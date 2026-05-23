using Company.App.Identity.Application;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Company.App.Identity.Infrastructure;

public sealed class IdentityModuleInitializer : IIdentityModuleInitializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentityModuleInitializer> _logger;
    private readonly IdentitySeedOptions _seedOptions;

    public IdentityModuleInitializer(
        IServiceProvider serviceProvider,
        ILogger<IdentityModuleInitializer> logger,
        IOptions<IdentitySeedOptions> seedOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _seedOptions = seedOptions.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Identity] Running module initializer. Seed email: {Email}", _seedOptions.AdminEmail);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("[Identity] Database migration complete.");

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(_seedOptions.AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(_seedOptions.AdminRole));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Failed to create '{_seedOptions.AdminRole}' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            _logger.LogInformation("[Identity] Admin role '{Role}' created.", _seedOptions.AdminRole);
        }
        else
        {
            _logger.LogInformation("[Identity] Admin role '{Role}' already exists.", _seedOptions.AdminRole);
        }

        var adminUser = await userManager.FindByEmailAsync(_seedOptions.AdminEmail);
        if (adminUser is null)
        {
            _logger.LogInformation("[Identity] Admin user not found - creating.");
            adminUser = new ApplicationUser
            {
                UserName = _seedOptions.AdminEmail,
                Email = _seedOptions.AdminEmail,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(adminUser, _seedOptions.AdminPassword);
            if (!userResult.Succeeded)
                throw new InvalidOperationException($"Failed to create seeded admin user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            await userManager.SetLockoutEnabledAsync(adminUser, false);
            _logger.LogInformation("[Identity] Admin user created successfully.");
        }
        else
        {
            _logger.LogInformation("[Identity] Admin user found (Id={Id}). Verifying state.", adminUser.Id);

            // Force-clear lockout directly in the database to bypass any EF tracking / UserManager caching issues
            var affected = await dbContext.Users
                .Where(u => u.Id == adminUser.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.LockoutEnd, (DateTimeOffset?)null)
                    .SetProperty(u => u.LockoutEnabled, false)
                    .SetProperty(u => u.AccessFailedCount, 0)
                    .SetProperty(u => u.EmailConfirmed, true)
                    .SetProperty(u => u.UserName, _seedOptions.AdminEmail)
                    .SetProperty(u => u.NormalizedUserName, _seedOptions.AdminEmail.ToUpperInvariant())
                    .SetProperty(u => u.Email, _seedOptions.AdminEmail)
                    .SetProperty(u => u.NormalizedEmail, _seedOptions.AdminEmail.ToUpperInvariant()),
                cancellationToken);

            _logger.LogInformation("[Identity] Admin user direct DB update affected {Count} row(s). Lockout cleared.", affected);

            // Re-fetch to get the latest state for password check
            adminUser = (await userManager.FindByEmailAsync(_seedOptions.AdminEmail))!;

            var passwordMatches = await userManager.CheckPasswordAsync(adminUser, _seedOptions.AdminPassword);
            _logger.LogInformation("[Identity] Admin password check result: {Result}", passwordMatches ? "matches" : "mismatch - resetting");
            if (!passwordMatches)
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetPasswordResult = await userManager.ResetPasswordAsync(adminUser, resetToken, _seedOptions.AdminPassword);
                if (!resetPasswordResult.Succeeded)
                    throw new InvalidOperationException($"Failed to reset seeded admin password: {string.Join(", ", resetPasswordResult.Errors.Select(e => e.Description))}");
                _logger.LogInformation("[Identity] Admin password reset successfully.");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, _seedOptions.AdminRole))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, _seedOptions.AdminRole);
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to assign seeded admin role: {string.Join(", ", addToRoleResult.Errors.Select(error => error.Description))}");
            }
        }

        _logger.LogInformation("Identity module initialized. Seeded admin email: {AdminEmail}", _seedOptions.AdminEmail);
    }
}

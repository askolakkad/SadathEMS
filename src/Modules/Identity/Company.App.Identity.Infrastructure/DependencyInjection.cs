using BuildingBlocks.Infrastructure.Extensions;
using Company.App.Identity.Application;
using Company.App.Identity.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.App.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdentityStorageOptions>(configuration.GetSection(IdentityStorageOptions.SectionName));
        services.Configure<IdentitySeedOptions>(configuration.GetSection(IdentitySeedOptions.SectionName));

        var storageOptions = configuration.GetSection(IdentityStorageOptions.SectionName).Get<IdentityStorageOptions>();
        var connectionString = storageOptions?.ConnectionString
            ?? configuration.GetConnectionString("PostgreSqlConnection")
            ?? configuration.GetConnectionString("IdentityConnection")
            ?? "Host=localhost;Port=5432;Database=SadathEMS.Identity;Username=postgres;Password=postgres";

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/access-denied";
            });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/access-denied";
        });

        services.AddAuthorization();
        services.AddModuleMappings(typeof(UserDto).Assembly);
        services.AddScoped<IIdentityAdministrationService, IdentityAdministrationService>();
        services.AddScoped<IIdentityAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<IIdentityModuleInitializer, IdentityModuleInitializer>();
        services.AddHostedService<IdentityModuleInitializationHostedService>();

        return services;
    }
}

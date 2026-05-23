using AutoMapper;
using Company.App.Identity.Application;
using Company.App.Identity.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Company.App.Identity.Infrastructure;

public sealed class IdentityAdministrationService : IIdentityAdministrationService
{
    private readonly IdentityDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public IdentityAdministrationService(
        IdentityDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    public async Task<IdentityAdministrationSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var userSummaries = new List<IdentityUserSummary>(users.Count);
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userSummaries.Add(new IdentityUserSummary(
                user.Id,
                user.Email ?? string.Empty,
                user.EmailConfirmed,
                userRoles.OrderBy(r => r).ToArray()));
        }

        var roles = await _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var roleGroups = await _dbContext.RoleGroups
            .OrderBy(rg => rg.Name)
            .ToListAsync(cancellationToken);

        return new IdentityAdministrationSnapshot(
            userSummaries,
            _mapper.Map<List<IdentityRoleSummary>>(roles),
            _mapper.Map<List<RoleGroupDto>>(roleGroups).Select(rg => rg.Name).ToList());
    }
}

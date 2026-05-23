using AutoMapper;
using BuildingBlocks.Core.Mapping;
using Microsoft.AspNetCore.Identity;

namespace Company.App.Identity.Application;

public sealed record IdentityLoginRequest(string Email, string Password, bool RememberMe);

public sealed record IdentityLoginResult(bool Succeeded, bool IsLockedOut);

public interface IIdentityAuthenticationService
{
    Task<IdentityLoginResult> PasswordSignInAsync(IdentityLoginRequest request, CancellationToken cancellationToken = default);

    Task SignOutAsync();
}

public interface IIdentityModuleInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public sealed class IdentityUserSummary : IMapFrom<ApplicationUserSource>
{
    public IdentityUserSummary() { }

    public IdentityUserSummary(string id, string email, bool emailConfirmed, IReadOnlyList<string> roles)
    {
        Id = id;
        Email = email;
        EmailConfirmed = emailConfirmed;
        Roles = roles;
    }

    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];

    public void Mapping(Profile profile) =>
        profile.CreateMap<ApplicationUserSource, IdentityUserSummary>();
}

/// <summary>Projection-safe source record — mirrors ApplicationUser fields needed for IdentityUserSummary.</summary>
public sealed record ApplicationUserSource(string Id, string Email, bool EmailConfirmed);

public sealed class IdentityRoleSummary : IMapFrom<IdentityRole>
{
    public IdentityRoleSummary() { }

    public IdentityRoleSummary(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public void Mapping(Profile profile) =>
        profile.CreateMap<IdentityRole, IdentityRoleSummary>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty));
}

public sealed record IdentityAdministrationSnapshot(
    IReadOnlyList<IdentityUserSummary> Users,
    IReadOnlyList<IdentityRoleSummary> Roles,
    IReadOnlyList<string> RoleGroups);

public interface IIdentityAdministrationService
{
    Task<IdentityAdministrationSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

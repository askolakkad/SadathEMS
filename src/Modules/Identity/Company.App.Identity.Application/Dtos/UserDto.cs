using AutoMapper;
using BuildingBlocks.Core.Mapping;

namespace Company.App.Identity.Application.Dtos;

/// <summary>
/// Flat user representation used across Identity UI and API responses.
/// Mapped from <c>ApplicationUser</c> via AutoMapper.
/// </summary>
public sealed class UserDto : IMapFrom<UserDtoSource>
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<UserDtoSource, UserDto>();
}

/// <summary>
/// Projection-safe intermediate used when mapping from EF queries.
/// Keeps <c>UserDto</c> independent of the Infrastructure <c>ApplicationUser</c> type.
/// </summary>
public sealed class UserDtoSource
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
}

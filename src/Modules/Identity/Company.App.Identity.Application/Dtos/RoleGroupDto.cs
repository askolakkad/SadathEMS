using AutoMapper;
using BuildingBlocks.Core.Mapping;
using Company.App.Identity.Domain;

namespace Company.App.Identity.Application.Dtos;

/// <summary>
/// Role group representation used in Identity administration views.
/// Mapped from the <see cref="RoleGroup"/> domain entity.
/// </summary>
public sealed class RoleGroupDto : IMapFrom<RoleGroup>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public void Mapping(Profile profile) =>
        profile.CreateMap<RoleGroup, RoleGroupDto>();
}

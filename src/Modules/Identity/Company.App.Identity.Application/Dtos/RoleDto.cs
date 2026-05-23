using AutoMapper;
using BuildingBlocks.Core.Mapping;
using Microsoft.AspNetCore.Identity;

namespace Company.App.Identity.Application.Dtos;

/// <summary>
/// Role representation used in Identity administration views.
/// </summary>
public sealed class RoleDto : IMapFrom<IdentityRole>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public void Mapping(Profile profile) =>
        profile.CreateMap<IdentityRole, RoleDto>();
}

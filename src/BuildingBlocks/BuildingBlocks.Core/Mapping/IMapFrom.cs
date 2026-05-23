using AutoMapper;

namespace BuildingBlocks.Core.Mapping;

/// <summary>
/// Implement on a DTO/model class to declare it maps from <typeparamref name="TSource"/>.
/// The mapping is registered automatically via <see cref="ModuleMappingProfile"/>.
/// </summary>
public interface IMapFrom<TSource>
{
    /// <summary>
    /// Override to customise the mapping. The default implementation creates a
    /// standard AutoMapper <c>CreateMap&lt;TSource, TDestination&gt;()</c>.
    /// </summary>
    void Mapping(Profile profile) =>
        profile.CreateMap(typeof(TSource), GetType());
}

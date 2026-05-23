using System.Reflection;
using BuildingBlocks.Infrastructure.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Extensions;

public static class MappingServiceExtensions
{
    /// <summary>
    /// Registers AutoMapper with a <see cref="ModuleMappingProfile"/> that scans
    /// <paramref name="assembly"/> for all <c>IMapFrom&lt;T&gt;</c> implementations.
    ///
    /// Call this once per module inside its <c>AddXxxInfrastructure</c> method:
    ///   services.AddModuleMappings(typeof(SomeDto).Assembly);
    /// </summary>
    public static IServiceCollection AddModuleMappings(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddAutoMapper(cfg =>
            cfg.AddProfile(new ModuleMappingProfile(assembly)));

        return services;
    }
}

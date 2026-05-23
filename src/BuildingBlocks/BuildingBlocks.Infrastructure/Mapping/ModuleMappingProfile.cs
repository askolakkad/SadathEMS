using System.Reflection;
using AutoMapper;
using BuildingBlocks.Core.Mapping;

namespace BuildingBlocks.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile that auto-discovers all <see cref="IMapFrom{T}"/> implementations
/// in the supplied assembly and registers their mappings.
///
/// Usage per module:
///   services.AddAutoMapper(cfg => cfg.AddProfile(new ModuleMappingProfile(typeof(MyDto).Assembly)));
/// </summary>
public sealed class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var mappingTypes = assembly.GetExportedTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.GetInterfaces()
                             .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType));

        foreach (var type in mappingTypes)
        {
            var instance = Activator.CreateInstance(type);

            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType);

            foreach (var iface in interfaces)
            {
                var mappingMethod = iface.GetMethod(nameof(IMapFrom<object>.Mapping))
                                    ?? mapFromType.MakeGenericType(iface.GetGenericArguments())
                                                  .GetMethod(nameof(IMapFrom<object>.Mapping));

                mappingMethod?.Invoke(instance, [this]);
            }
        }
    }
}

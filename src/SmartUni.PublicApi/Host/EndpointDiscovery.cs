using System.Reflection;

namespace SmartUni.PublicApi.Host;

public static class EndpointDiscovery
{
    // ref: https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/blob/main/src/VerticalSliceArchitectureTemplate/Host/FeatureDiscovery.cs
    private static readonly Type EndpointType = typeof(IEndpoint);

    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        var endpointTypes = GetEndpointTypes(assemblies);

        foreach (var type in endpointTypes)
        {
            var method = GetMapEndpointsMethod(type);
            method?.Invoke(null, [endpoints]);
        }
    }

    private static IEnumerable<Type> GetEndpointTypes(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => EndpointType.IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false });
    }

    private static MethodInfo? GetMapEndpointsMethod(IReflect type)
    {
        return type.GetMethod(nameof(IEndpoint.MapEndpoint),
            BindingFlags.Static | BindingFlags.Public);
    }
}
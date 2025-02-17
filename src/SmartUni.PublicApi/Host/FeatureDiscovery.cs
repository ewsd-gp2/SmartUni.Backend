using System.Reflection;

namespace SmartUni.PublicApi.Host;

public static class FeatureDiscovery
{
    // ref: https://github.com/SSWConsulting/SSW.VerticalSliceArchitecture/blob/main/src/VerticalSliceArchitectureTemplate/Host/FeatureDiscovery.cs
    private static readonly Type ModuleType = typeof(IFeature);

    public static void ConfigureFeatures(this IServiceCollection services, IConfiguration config,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        var moduleTypes = GetFeatureTypes(assemblies);

        foreach (var type in moduleTypes)
        {
            var method = GetConfigureServicesMethod(type);
            method?.Invoke(null, [services, config]);
        }
    }

    private static IEnumerable<Type> GetFeatureTypes(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => ModuleType.IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false });
    }

    private static MethodInfo? GetConfigureServicesMethod(Type type)
    {
        return type.GetMethod(nameof(IFeature.ConfigureServices),
            BindingFlags.Static | BindingFlags.Public);
    }
}
namespace SmartUni.PublicApi.Common.Features;

public interface IFeature
{
    static abstract void ConfigureServices(IServiceCollection services, IConfiguration config);
}
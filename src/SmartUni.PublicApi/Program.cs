using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Host;
using SmartUni.PublicApi.Persistence;
using System.Reflection;

Assembly appAssembly = Assembly.GetExecutingAssembly();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);
builder.Services.AddOpenApi();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDbContext<SmartUniDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("SmartUniDb"));
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
        options.WithTheme(ScalarTheme.Kepler);
    });
}

app.UseProductionExceptionHandler();
app.RegisterEndpoints(appAssembly);
// app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseHsts();
app.Run();
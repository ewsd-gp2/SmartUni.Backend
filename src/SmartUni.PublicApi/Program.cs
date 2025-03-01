using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Host;
using System.Reflection;

Assembly appAssembly = Assembly.GetExecutingAssembly();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);
builder.Services.AddOpenApi();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDatabase(builder.Configuration);

WebApplication app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    options.WithTheme(ScalarTheme.Kepler);
});
app.ApplyMigrations();

app.UseProductionExceptionHandler();
app.RegisterEndpoints(appAssembly);
// app.UseSerilogRequestLogging();
// app.UseHttpsRedirection();
// app.UseHsts();
app.Run();
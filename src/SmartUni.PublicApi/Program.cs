using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Host;
using System.Reflection;

Assembly appAssembly = Assembly.GetExecutingAssembly();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
const string corsPolicyName = "localhost";

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName,
        policyBuilder =>
        {
            policyBuilder.WithOrigins(builder.Configuration["ClientAppUrl"]!).AllowCredentials().AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddDatabase(builder.Configuration);

WebApplication app = builder.Build();

app.ApplyMigrations();
// app.UseHttpsRedirection();
// app.UseHsts();
// app.UseSerilogRequestLogging();
app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    options.WithTheme(ScalarTheme.Kepler);
});
app.RegisterEndpoints(appAssembly);
app.UseCors(corsPolicyName);
app.Run();
using System.Reflection;
using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Host;
using SmartUni.PublicApi.Persistence;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);
builder.Services.AddOpenApi();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddDbContext<SmartUniDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("SmartUniDb"));
    optionsBuilder.UseSnakeCaseNamingConvention();
});

var app = builder.Build();
app.MapGet("/", () => Log.Information("Web server started!"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
        options.WithTheme(ScalarTheme.Alternate);
    });
}

app.UseProductionExceptionHandler();
app.RegisterEndpoints(appAssembly);
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.Run();
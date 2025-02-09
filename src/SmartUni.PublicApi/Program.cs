using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();
app.UseSerilogRequestLogging();
app.MapGet("/", () => Log.Information("Web server started!"));

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();
app.Run();
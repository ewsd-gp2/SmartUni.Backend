using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Host;
using SmartUni.PublicApi.Persistence;
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
builder.Services.AddIdentity<BaseUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SmartUniDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            ValidIssuer = "http://localhost:7142",
            ValidAudience = "http://localhost:5173",
            IssuerSigningKey = new SymmetricSecurityKey("DoNotShareThisSuperSecretKey!@SDF123!@#"u8.ToArray())
        };
    });

builder.Services.AddAuthorization();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
});

WebApplication app = builder.Build();

app.ApplyMigrations();
app.UseAuthentication();
app.UseAuthorization();
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
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Extensions;
using SmartUni.PublicApi.Features.Email;
using SmartUni.PublicApi.Features.Message;
using SmartUni.PublicApi.Features.Message.Hubs;
using SmartUni.PublicApi.Host;
using SmartUni.PublicApi.Persistence;
using System.Reflection;
using IEmailSender = SmartUni.PublicApi.Features.Email.Interface.IEmailSender;

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

builder.Services.AddAuthentication(authOptions =>
    {
        authOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://localhost:7142",
            ValidAudience = "http://localhost:5173",
            IssuerSigningKey =
                new SymmetricSecurityKey("DoNotShareThisSuperSecretKey!@SDF123!@#"u8.ToArray())
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["accessToken"];

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorizationBuilder()
    .AddPolicy("api", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
});
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSignalR();
builder.Services.AddMediatR(typeof(Program).Assembly);
builder.Services.AddSingleton<sharedDB>();
WebApplication app = builder.Build();

app.ApplyMigrations();
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        SmartUniDbContext context = services.GetRequiredService<SmartUniDbContext>();
        UserManager<BaseUser> userManager = services.GetRequiredService<UserManager<BaseUser>>();
        RoleManager<IdentityRole<Guid>>
            roleManager =
                services.GetRequiredService<RoleManager<IdentityRole<Guid>>>(); // If you are using RoleManager

        await DbInitializer.InitializeAsync(context, userManager, roleManager); // Call your seeding method
    }
    catch (Exception ex)
    {
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseCors();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/ChatHub")
    .RequireCors();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithDefaultHttpClient(ScalarTarget.JavaScript, ScalarClient.Axios);
    options.WithTheme(ScalarTheme.Kepler);
});
app.RegisterEndpoints(appAssembly);
app.UseCors(corsPolicyName);
app.Run();
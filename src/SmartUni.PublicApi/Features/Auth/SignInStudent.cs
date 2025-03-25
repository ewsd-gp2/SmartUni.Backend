using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Auth
{
    public class SignInStudent
    {
        private sealed record Request(string Email, string Password);
        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/signin/student",
                        async Task<IResult> (
                            Request request, [FromQuery] bool? useCookies, [FromQuery] bool? useSessionCookies,
                            [FromServices] IServiceProvider sp, HttpContext context, SmartUniDbContext dbContext,
                            ILogger<Endpoint> logger) =>
                        {
                            logger.LogInformation("Student log in requested with email: {TutorEmail}", request.Email);

                            UserManager<BaseUser> userManager = sp.GetRequiredService<UserManager<BaseUser>>();

                            BaseUser? user = await userManager.Users.Include(x => x.Tutor)
                                .FirstOrDefaultAsync(x => x.Email == request.Email);
                            if (user != null)
                            {
                                if (!await userManager.CheckPasswordAsync(user, request.Password))
                                {
                                    logger.LogInformation("Signed in with incorrect password or email.");
                                    return Results.Unauthorized();
                                }

                                string token = TokenHelper.GenerateToken(user, "Student");
                                TokenHelper.SetTokensInsideCookie(token, context);

                                user.LastLoginDate = DateTime.UtcNow;
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                return Results.Unauthorized();
                            }

                            return Results.Ok();
                        })
                    .WithDescription("Sign in Student")
                    .WithTags("Auth");
            }
        }
    }
}

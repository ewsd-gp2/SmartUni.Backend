using Microsoft.AspNetCore.Identity;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Auth
{
    public class SignInTutor
    {
        private sealed record Request(string Email, string Password);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/signin/tutor",
                        async (Request request, SignInManager<BaseUser> signInManager,
                            UserManager<BaseUser> userManager, ILogger<Endpoint> logger,
                            SmartUniDbContext dbContext, CancellationToken cancellationToken) =>
                        {
                            BaseUser? user = await userManager.Users.Include(x => x.Tutor)
                                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
                            if (user == null)
                            {
                                logger.LogInformation("User with email {Email} not found", request.Email);
                                return TypedResults.Unauthorized();
                            }

                            SignInResult signinResult =
                                await signInManager.PasswordSignInAsync(user, request.Password, false, true);
                            if (!signinResult.Succeeded)
                            {
                                logger.LogInformation("Failed to sign in for user with {Email}", request.Email);
                                return TypedResults.Unauthorized();
                            }

                            logger.LogInformation("User with email {Email} signed in", request.Email);
                            Tutor.Tutor? tutor = user!.Tutor;

                            string token = TokenHelper.GenerateToken(user);

                            return Results.Ok(new { access_token = token });
                        })
                    .WithDescription("Sign in tutor")
                    .WithTags("Auth");
            }
        }
    }
}
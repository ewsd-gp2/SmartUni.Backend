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
                        async (Request request, UserManager<BaseUser> userManager, ILogger<Endpoint> logger,
                            SmartUniDbContext dbContext) =>
                        {
                            BaseUser? user = await userManager.FindByEmailAsync(request.Email);
                            if (user == null)
                            {
                                logger.LogInformation("User with email {Email} not found", request.Email);
                                return TypedResults.Unauthorized();
                            }

                            bool result = await userManager.CheckPasswordAsync(user, request.Password);
                            if (!result)
                            {
                                logger.LogInformation("Password for user with email {Email} is incorrect",
                                    request.Email);
                                return TypedResults.Unauthorized();
                            }

                            logger.LogInformation("User with email {Email} signed in", request.Email);

                            Tutor.Tutor? tutor =
                                await dbContext.Tutor.FirstOrDefaultAsync(x => x.IdentityId == user.Id);

                            string token = TokenHelper.GenerateToken(user.Id, tutor!.Id, tutor.Name,
                                user.Email!,
                                user.LastActiveDate?.Date, "Tutor");

                            return Results.Ok(new { access_token = token });
                        })
                    .WithDescription("Sign in tutor")
                    .WithTags("Auth");
            }
        }
    }
}
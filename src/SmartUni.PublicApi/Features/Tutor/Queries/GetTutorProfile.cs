using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;
using System.Net;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Tutor.Queries
{
    public class GetTutorProfile
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string? Email,
            string? PhoneNumber,
            string Gender,
            string Major,
            string UserCode,
            string? Image,
            bool IsFirstLoggedIn,
            DateTime? LastLoggedInDate,
            int InactiveDays);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/tutor/profile", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Response>(200)
                    .Produces<NotFoundResult>(404)
                    .WithTags(nameof(Tutor));
            }

            private static async Task<IResult> HandleAsync(ClaimsPrincipal claims,
                [FromServices] SmartUniDbContext dbContext, [FromServices] ILogger<Endpoint> logger)
            {
                Guid tutorId = Guid.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                          throw new InvalidOperationException("No user id found"));

                logger.LogInformation("Submitted request for tutor profile with {TutorId}", tutorId);
                Tutor? tutor = await dbContext.Tutor.Where(x => x.Id == tutorId).Include(x => x.Identity)
                    .FirstOrDefaultAsync();
                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID {TutorId} not found", tutorId);
                    return TypedResults.NotFound();
                }

                Response response = new Response(tutorId, tutor.Name, tutor.Identity.Email, tutor.Identity.PhoneNumber,
                    tutor.Gender.ToString(), tutor.Major.ToString(), tutor.UserCode,
                    tutor.Image is null ? null : Convert.ToBase64String(tutor.Image),
                    tutor.Identity.IsFirstLogin, tutor.Identity.LastLoginDate ?? null,
                    tutor.Identity.LastLoginDate.HasValue
                        ? DateTime.UtcNow.Subtract(tutor.Identity.LastLoginDate.Value).Days
                        : 0);
                logger.LogInformation("Successfully retrieved tutor profile with {TutorId}", tutorId);
                return TypedResults.Ok(response);
            }
        }
    }
}
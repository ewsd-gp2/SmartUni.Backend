using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Queries
{
    public class GetAllTutors
    {
        private record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            Enums.GenderType Gender,
            Enums.MajorType Major,
            string Image,
            string UserCode);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/tutor", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all tutors");

                IEnumerable<Response> tutors = await dbContext.Tutor
                    .Include(x => x.Identity)
                    .Where(x => !x.IsDeleted)
                    .Select(t => new Response(t.Id, t.Name, t.Identity.Email!, t.Identity.PhoneNumber!,
                        t.Gender,
                        t.Major, t.Image == null ? string.Empty : Convert.ToBase64String(t.Image), t.UserCode))
                    .ToListAsync(cancellationToken);

                if (!tutors.Any())
                {
                    logger.LogWarning("No tutors found");
                    return TypedResults.Ok();
                }

                logger.LogInformation("Successfully retrieved all tutors. Found {TutorCount} tutors", tutors.Count());
                return TypedResults.Ok(tutors);
            }
        }
    }
}
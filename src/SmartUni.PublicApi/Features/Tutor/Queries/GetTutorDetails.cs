using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Tutor.Queries
{
    public class GetTutorDetails
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            string UserCode);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/tutor/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Tutor));
            }

            private static async Task<Results<Ok<Response>, NotFound>> HandleAsync(
                Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for tutor with ID: {Id}", id);

                Tutor? tutor = await dbContext.Tutor.Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (tutor is null)
                {
                    logger.LogWarning("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                Response response = new(tutor.Id, tutor.Name, tutor.Identity.Email!,
                    tutor.Identity.PhoneNumber!,
                    tutor.Gender.ToString(),
                    tutor.Major.ToString(), tutor.UserCode);
                logger.LogInformation("Successfully fetched details for tutor with ID: {Id} with response: {Response}",
                    id, response);
                return TypedResults.Ok(response);
            }
        }
    }
}
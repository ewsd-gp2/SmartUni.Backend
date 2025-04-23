using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Allocation.Queries
{
    public class AllocationByTutor
    {
        private sealed record Response(
            Guid Id,
            string StudentName,
            Guid StudentID,
            string Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/allocationByTutor/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .RequireAuthorization("api")
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<Ok<List<Response>>, NotFound>> HandleAsync(
    Guid id,
    SmartUniDbContext dbContext,
    ILogger<Endpoint> logger,
    CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for allocation with tutor ID: {Id}", id);

                var allocations = await dbContext.Allocation
                    .Where(a => a.TutorId == id && !a.IsDeleted)
                    .Include(a => a.Student)
                    .Select(a => new Response(
                        a.Id,
                        a.Student.Name,
                        a.StudentId,
                        Convert.ToBase64String(a.Student.Image ?? Array.Empty<byte>())))
                    .ToListAsync(cancellationToken);

                if (allocations.Any())
                {
                    logger.LogInformation("Successfully fetched {Count} allocations for tutor ID: {Id}", allocations.Count, id);
                    return TypedResults.Ok(allocations);
                }

                logger.LogWarning("No allocations found for tutor ID: {Id}", id);
                return TypedResults.NotFound();
            }

        }
    }
}
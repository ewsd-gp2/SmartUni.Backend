using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Allocation.Queries
{
    public class GetAllAllocation
    {
        private record Response(Guid Id, Guid TutorID, Guid StudentID, bool Is_Deleted, bool Is_Allocated);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/getAllocationList", HandleAsync)
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Allocation));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all allocations");

                IEnumerable<Response> allocation = await dbContext.Allocations
                    .Select(t => new Response(t.Id, t.TutorId, t.StudentId, t.IsDeleted, t.IsAllocated))
                    .ToListAsync(cancellationToken);

                if (!allocation.Any())
                {
                    logger.LogWarning("No staffs found");
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Successfully retrieved all allocations. Found {AllocationCount} allocations",
                    allocation.Count());
                return TypedResults.Ok(allocation);
            }
        }
    }
}
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Dashboard.Queries
{
    public class GetTutorDashboard
    {
        private sealed record Response(
            Guid TutorId,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            List<AllocationResponse> Students);

        private sealed record AllocationResponse(Guid AllocationId, Guid StudentId, string Name);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/dashboard/tutor/{id:guid}/",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .WithDescription("Get dashboard details for a tutor")
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags("Dashboard");
            }

            private static async Task<IResult> HandleAsync(
                Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for tutor with ID: {Id}", id);

                Tutor.Tutor? tutor = await dbContext.Tutor.Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (tutor is null)
                {
                    logger.LogInformation("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                List<AllocationResponse> allocations = dbContext.Allocation.Where(a => a.TutorId == id)
                    .Include(a => a.Student)
                    .Select(a => new AllocationResponse(a.Id, a.StudentId, a.Student.Name)).ToList();
                Response response = new(tutor!.Id, tutor.Name, tutor.Identity.Email!,
                    tutor.Identity.PhoneNumber!, tutor.Gender.ToString(),
                    tutor.Major.ToString(), allocations);
                logger.LogInformation("Successfully fetched details for tutor with ID: {Id} with response: {Response}",
                    id, response);
                return TypedResults.Ok(response);
            }
        }
    }
}
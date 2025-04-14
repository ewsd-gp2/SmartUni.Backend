using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Dashboard.Queries
{
    public class GetStudentDashBoard
    {
        private sealed record Response(
            Guid StudentId,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            byte[] Profile,
            AllocationResponse allocation);

        private sealed record AllocationResponse(Guid AllocationId, Guid TutorId, string Name);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/dashboard/student/{id:guid}/",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .WithDescription("Get dashboard details for a student")
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
                logger.LogInformation("Fetching details for student with ID: {Id}", id);

                var student = await dbContext.Student
                    .Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (student is null)
                {
                    logger.LogInformation("Student with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                // Get allocation and tutor (if any)
                var allocation = await dbContext.Allocation
                    .Where(a => a.StudentId == id)
                    .Select(a => new { a.Id, a.TutorId })
                    .FirstOrDefaultAsync(cancellationToken);

                AllocationResponse? allocationResponse = null;

                if (allocation != null)
                {
                    var tutor = await dbContext.Tutor
                        .Where(a => a.Id == allocation.TutorId)
                        .Select(a => new { a.Name })
                        .FirstOrDefaultAsync(cancellationToken);

                    allocationResponse = new AllocationResponse(
                        allocation.Id,
                        allocation.TutorId,
                        tutor?.Name ?? "Unknown"
                    );
                }

                var response = new Response(
                    student.Id,
                    student.Name,
                    student.Identity.Email!,
                    student.Identity.PhoneNumber!,
                    student.Gender.ToString(),
                    student.Major.ToString(),
                    student.Image,
                    allocationResponse
                );

                logger.LogInformation("Successfully fetched details for student with ID: {Id}", id);
                return TypedResults.Ok(response);
            }

        }
    }
}

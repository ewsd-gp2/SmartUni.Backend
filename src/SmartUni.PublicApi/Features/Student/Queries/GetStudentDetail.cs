using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Student.Queries
{
    public class GetStudentDetail
    {
        private record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            bool IsDeleted,
            bool IsAllocated);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/getStudentDetail/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<Ok<Response>, NotFound>> HandleAsync(
                Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for student with ID: {Id}", id);

                Student? student = await dbContext.Student.FindAsync(id, cancellationToken);
                if (student != null)
                {
                    // Check if the student is allocated by looking for an allocation record
                    bool isAllocated = await dbContext.Allocations
                        .AnyAsync(allocation => allocation.StudentId == student.Id, cancellationToken);

                    Response response = new(
                        student.Id,
                        student.Name,
                        student.Email,
                        student.PhoneNumber,
                        student.Gender,
                        student.IsDeleted,
                        isAllocated // Set isAllocated to true if the student has an allocation, false otherwise
                    );

                    //return response;
                    //Response response = new(student.Id, student.Name, student.Email, student.PhoneNumber, student.gender, student.is_deleted);
                    logger.LogInformation("Successfully fetched details for student with ID: {Id}", id);
                    return TypedResults.Ok(response);
                }

                logger.LogWarning("Student with ID: {Id} not found", id);
                return TypedResults.NotFound();
            }
        }
    }
}
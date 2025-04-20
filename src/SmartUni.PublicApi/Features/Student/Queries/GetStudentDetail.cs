using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
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
            Enums.GenderType Gender,
            Enums.MajorType Major,
            Guid? AllocationID,
            bool IsAllocated,
            string UserCode,
            string Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/student/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .RequireAuthorization("api")
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

                Student? student = await dbContext.Student.Include(x => x.Identity).Where(x => !x.IsDeleted).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (student != null)
                {
                    bool isAllocated = student.Allocation?.Id != null && student.Allocation?.Id != Guid.Empty;
                    Response response = new(
                        student.Id,
                        student.Name,
                        student.Identity.Email,
                        student.Identity.PhoneNumber,
                        student.Gender,
                        student.Major,
                        student.Allocation?.Id,
                        isAllocated,// Set isAllocated to true if the student has an allocation, false otherwise
                    student.UserCode,
                    student.Image is null ? string.Empty : Convert.ToBase64String(student.Image)
                    );

                    logger.LogInformation("Successfully fetched details for student with ID: {Id}", id);
                    return TypedResults.Ok(response);
                }

                logger.LogWarning("Student with ID: {Id} not found", id);
                return TypedResults.NotFound();
            }
        }
    }
}
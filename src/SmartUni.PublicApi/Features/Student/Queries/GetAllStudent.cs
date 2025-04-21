using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Student.Queries
{
    public class GetAllStudent
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
                endpoints.MapGet("/student", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all students");

                var student = await dbContext.Student
    .Include(s => s.Identity)
    .Include(s => s.Allocation)
    .Where(s => !s.IsDeleted)
    .Select(s => new
    {
        s.Id,
        s.Name,
        s.Identity.Email,
        s.Identity.PhoneNumber,
        s.Gender,
        s.Major,
        s.UserCode,
        IsAllocated = s.Allocation != null && s.Allocation.Id != Guid.Empty,
        s.Image
    })
    .ToListAsync();


                if (!student.Any())
                {
                    logger.LogWarning("No students found");
                    return TypedResults.NotFound();
                }
                logger.LogInformation("Successfully retrieved all students. Found {StudentCount} students",
                    student.Count());
                return TypedResults.Ok(student);
            }
        }
    }
}
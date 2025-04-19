using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Report.Queries
{
    public class StudentsWIthoutTutorReport
    {
        private record Response(
           Guid Id,
           string Name,
           string Email,
           string PhoneNumber,
           string Gender,
           string Major,
           Guid? AllocationID,
           bool IsAllocated,
           string UserCode,
           string? Image);


        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/studentsWithoutTutor", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Report));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all students without tutor");

                var student = await dbContext.Student
    .Include(s => s.Identity)
    .Include(s => s.Allocation)
    .Where(s => !s.IsDeleted && (s.Allocation == null || s.Allocation.IsDeleted))
    .Select(s => new
    {
        s.Id,
        s.Name,
        s.Identity.Email,
        s.Identity.PhoneNumber,
        Gender = char.ToUpper(s.Gender.ToString()[0]) + s.Gender.ToString().Substring(1).ToLower(),
        Major = char.ToUpper(s.Major.ToString()[0]) + s.Major.ToString().Substring(1).ToLower(),
        s.UserCode,
        IsAllocated = s.Allocation != null && s.Allocation.Id != Guid.Empty && s.Allocation.IsDeleted == false,
        s.Image
    })
    .ToListAsync();


                if (!student.Any())
                {
                    logger.LogWarning("No students without allocation found");
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Successfully retrieved all students. Found {StudentCount} students",
                    student.Count());
                return TypedResults.Ok(student);
            }
        }
    }
}
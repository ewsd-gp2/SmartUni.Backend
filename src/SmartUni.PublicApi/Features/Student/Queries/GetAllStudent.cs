using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
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
            bool IsDeleted,
            Guid? AllocationID,
            bool IsAllocated);


        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/student", HandleAsync)
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
                .Include(s => s.Allocation) // Eagerly load Allocation details
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Email,
                    s.PhoneNumber,
                    s.Gender,
                    s.Major,
                    s.IsDeleted,
                    s.AllocationID,
                    IsAllocated = s.AllocationID !=null && s.AllocationID != Guid.Empty
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
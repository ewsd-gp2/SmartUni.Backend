using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Student.Queries
{
    public class GetAllStudent
    {
        //private record Response(Guid Id, string Name, string Email, string PhoneNumber,string Gender,bool IsDeleted,bool isAllocated);
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
                endpoints.MapGet("/getStudentList", HandleAsync)
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Student));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all students");

                //IEnumerable<Response> student = await dbContext.Student
                //    .Select(t => new Response(t.Id, t.Name, t.Email, t.PhoneNumber,t.gender,t.is_deleted))
                //    .ToListAsync(cancellationToken);

                var student = await dbContext.Student
                    .GroupJoin(
                        dbContext.Allocations,
                        student => student.Id,
                        allocation => allocation.StudentId,
                        (student, allocations) => new { student, allocations } // GroupJoin returns a collection
                    )
                    .SelectMany(
                        x => x.allocations
                            .DefaultIfEmpty(), // Ensures all students are included, even if no allocation exists
                        (x, allocation) => new
                        {
                            x.student.Id,
                            x.student.Name,
                            x.student.Email,
                            x.student.PhoneNumber,
                            x.student.Gender,
                            x.student.IsDeleted,
                            IsAllocated = allocation != null ? allocation.IsAllocated : false // Handle null allocation
                        }
                    )
                    .ToListAsync(cancellationToken);

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
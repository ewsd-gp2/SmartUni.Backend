using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SmartUni.PublicApi.Persistence;
using System.Threading;

namespace SmartUni.PublicApi.Features.Student.Queries
{
    public class GetAllStudent
    {
        //private record Response(Guid Id, string Name, string Email, string PhoneNumber,string Gender,bool IsDeleted,bool isAllocated);
        private record Response(Guid Id, string Name, string Email, string PhoneNumber, string Gender, bool IsDeleted, bool IsAllocated);


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
          allocation => allocation.student_id,
          (student, allocations) => new { student, allocations } // GroupJoin returns a collection
      )
      .SelectMany(
          x => x.allocations.DefaultIfEmpty(), // Ensures all students are included, even if no allocation exists
          (x, allocation) => new
          {
              Id = x.student.Id,
              Name = x.student.Name,
              Email = x.student.Email,
              PhoneNumber = x.student.PhoneNumber,
              Gender = x.student.gender,
              IsDeleted = x.student.is_deleted,
              IsAllocated = allocation != null ? allocation.is_allocated : false // Handle null allocation
          }
      )
      .ToListAsync(cancellationToken);

                if (!student.Any())
                {
                    logger.LogWarning("No students found");
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Successfully retrieved all students. Found {StudentCount} students", student.Count());
                return TypedResults.Ok(student);
            }
        }
    }
}

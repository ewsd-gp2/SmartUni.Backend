using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Staff.Queries
{
    public class GetAllStaff
    {
        private record Response(Guid Id, string Name, string Email, string PhoneNumber,string Gender,bool IsDeleted);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/getStaffList", HandleAsync)
                    .Produces<Results<IResult, NotFound>>()
                    .WithTags(nameof(Staff));
            }

            private static async Task<Results<IResult, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to get all staffs");

                IEnumerable<Response> staff = await dbContext.Staff
                    .Select(t => new Response(t.Id, t.Name, t.Email, t.PhoneNumber,t.gender,t.is_deleted))
                    .ToListAsync(cancellationToken);

                if (!staff.Any())
                {
                    logger.LogWarning("No staffs found");
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Successfully retrieved all staffs. Found {TutorCount} tutors", staff.Count());
                return TypedResults.Ok(staff);
            }
        }
    }
}

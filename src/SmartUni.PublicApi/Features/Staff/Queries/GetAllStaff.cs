using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Staff.Queries
{
    public class GetAllStaff
    {
        private record Response(Guid Id, string Name, string Email, string PhoneNumber, Enums.GenderType Gender,string UserCode,string Role,string Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/staff", HandleAsync)
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
                    .Include(x => x.Identity)
                .Where(x => !x.IsDeleted)
                .Select(t => new Response(t.Id, t.Name, t.Identity.Email, t.Identity.PhoneNumber, t.Gender,t.UserCode,t.Identity.Role.ToString(), t.Image == null ? string.Empty : Convert.ToBase64String(t.Image)))
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
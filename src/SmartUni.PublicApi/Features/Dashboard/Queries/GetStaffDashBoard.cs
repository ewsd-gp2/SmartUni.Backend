
using global::SmartUni.PublicApi.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Dashboard.Queries
{
    public class GetStaffDashboard
    {
        private sealed record Response(
            Guid StaffId,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            byte[] Profile);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/dashboard/staff/{id:guid}/",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .WithDescription("Get dashboard details for a staff member")
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
                logger.LogInformation("Fetching details for staff with ID: {Id}", id);

                var staff = await dbContext.Staff
                    .Include(s => s.Identity)
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (staff is null)
                {
                    logger.LogInformation("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                var response = new Response(
                    staff.Id,
                    staff.Name,
                    staff.Identity.Email!,
                    staff.Identity.PhoneNumber!,
                    staff.Gender.ToString(),
                    staff.Image
                );

                logger.LogInformation("Successfully fetched details for staff with ID: {Id}", id);
                return TypedResults.Ok(response);
            }
        }
    }
}



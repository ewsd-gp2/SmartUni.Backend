using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Staff.Queries
{
    public class GetStaffDetail
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            bool IsDeleted);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/getStaffDetail/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .Produces<Ok<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Staff));
            }

            private static async Task<Results<Ok<Response>, NotFound>> HandleAsync(
                Guid id,
                SmartUniDbContext dbContext,
                ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching details for staff with ID: {Id}", id);

                Staff? staff = await dbContext.Staff.FindAsync([id], cancellationToken);
                if (staff is null)
                {
                    logger.LogWarning("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                Response response = new(staff.Id, staff.Name, staff.Email, staff.PhoneNumber, staff.Gender,
                    staff.IsDeleted);
                logger.LogInformation("Successfully fetched details for staff with ID: {Id}", id);
                return TypedResults.Ok(response);
            }
        }
    }
}
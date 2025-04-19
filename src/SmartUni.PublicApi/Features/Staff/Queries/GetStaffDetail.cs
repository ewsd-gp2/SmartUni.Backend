using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
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
            Enums.GenderType Gender,
            string UserCode,
            string Image);

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/staff/{id:guid}",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .RequireAuthorization("api")
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

                Staff? staff = await dbContext.Staff.Include(x => x.Identity).Where(x=>!x.IsDeleted).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (staff is null)
                {
                    logger.LogWarning("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                Response response = new(staff.Id, staff.Name, staff.Identity.Email, staff.Identity.PhoneNumber, staff.Gender,staff.UserCode, staff.Image is null ? string.Empty : Convert.ToBase64String(staff.Image));
                logger.LogInformation("Successfully fetched details for staff with ID: {Id}", id);
                return TypedResults.Ok(response);
            }
        }
    }
}
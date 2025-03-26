using Microsoft.AspNetCore.Http.HttpResults;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Staff.Commands
{
    public class DeleteStaff
    {
        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapDelete("/staff/{id:guid}", HandleAsync)
                     .RequireAuthorization("api")
                    .Produces<NotFound>()
                    .WithTags(nameof(Staff));
            }

            private static async Task<Results<NoContent, NotFound>> HandleAsync(
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext,
                ClaimsPrincipal claims,
                Guid id,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to delete staff with ID: {Id}", id);

                Staff? staff = await dbContext.Staff.FindAsync([id], cancellationToken);
                if (staff is null)
                {
                    logger.LogWarning("Staff with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                staff.DeleteStaff();
                staff.UpdatedOn = DateTime.UtcNow;
                staff.UpdatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier));
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Successfully deleted staff with ID: {Id}", id);
                return TypedResults.NoContent();
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Staff.Queries
{
    public class GetStaffProfile
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string? Email,
            string? PhoneNumber,
            string Gender,
            string UserCode,
            string? Image,
            bool IsFirstLoggedIn,
            DateTime? LastLoggedInDate,
            int InactiveDays);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/staff/profile", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Response>(200)
                    .Produces<NotFoundResult>(404)
                    .WithTags(nameof(Staff));
            }

            private static async Task<IResult> HandleAsync(ClaimsPrincipal claims,
                [FromServices] SmartUniDbContext dbContext, [FromServices] ILogger<Endpoint> logger)
            {
                Guid staffId = Guid.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                          throw new InvalidOperationException("No user id found"));

                logger.LogInformation("Submitted request for staff profile with {StaffId}", staffId);
                Staff? staff = await dbContext.Staff.Where(x => x.Id == staffId).Include(x => x!.Identity)
                    .FirstOrDefaultAsync();
                if (staff is null)
                {
                    logger.LogWarning("Staff with ID {StaffId} not found", staffId);
                    return TypedResults.NotFound();
                }

                Response response = new Response(staffId, staff.Name, staff.Identity.Email, staff.Identity.PhoneNumber,
                    staff.Gender.ToString(), staff.UserCode,
                    staff.Image is null ? null : Convert.ToBase64String(staff.Image),
                    staff.Identity.IsFirstLogin, staff.Identity.LastLoginDate ?? null,
                    staff.Identity.LastLoginDate.HasValue
                        ? DateTime.UtcNow.Subtract(staff.Identity.LastLoginDate.Value).Days
                        : 0);
                logger.LogInformation("Successfully retrieved staff profile with {StaffId}", staffId);
                return TypedResults.Ok(response);
            }
        }
    }
}
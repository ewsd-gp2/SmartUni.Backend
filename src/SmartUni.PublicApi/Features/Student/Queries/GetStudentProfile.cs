using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Student.Queries
{
    public class GetStudentProfile
    {
        private sealed record Response(
            Guid Id,
            string Name,
            string? Email,
            string? PhoneNumber,
            string Gender,
            string Major,
            string UserCode,
            string? Image,
            string Role,
            bool IsFirstLoggedIn,
            DateTime? LastLoggedInDate,
            int InactiveDays);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/student/profile", HandleAsync)
                    .RequireAuthorization("api")
                    .Produces<Response>(200)
                    .Produces<NotFoundResult>(404)
                    .WithTags(nameof(Student));
            }

            private static async Task<IResult> HandleAsync(ClaimsPrincipal claims,
                [FromServices] SmartUniDbContext dbContext, [FromServices] ILogger<Endpoint> logger)
            {
                Guid studentId = Guid.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                                          throw new InvalidOperationException("No user id found"));

                logger.LogInformation("Submitted request for student profile with {StudentId}", studentId);
                Student? student = await dbContext.Student.Where(x => x != null && x.Id == studentId).Include(x => x!.Identity)
                    .FirstOrDefaultAsync();
                if (student is null)
                {
                    logger.LogWarning("Student with ID {StudentId} not found", studentId);
                    return TypedResults.NotFound();
                }

                Response response = new Response(studentId, student.Name, student.Identity.Email, student.Identity.PhoneNumber,
                    student.Gender.ToString(), student.Major.ToString(), student.UserCode,
                    student.Image is null ? null : Convert.ToBase64String(student.Image),
                    student.Identity.Role.ToString(),
                    student.Identity.IsFirstLogin, student.Identity.LastLoginDate ?? null,
                    student.Identity.LastLoginDate.HasValue
                        ? DateTime.UtcNow.Subtract(student.Identity.LastLoginDate.Value).Days
                        : 0);
                logger.LogInformation("Successfully retrieved student profile with {StudentId}", studentId);
                return TypedResults.Ok(response);
            }
        }
    }
}
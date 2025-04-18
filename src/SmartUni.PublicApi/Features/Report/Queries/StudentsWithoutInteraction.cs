
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Features.Tutor;
using SmartUni.PublicApi.Persistence;
using System;

namespace SmartUni.PublicApi.Features.Report.Queries
{
    public class StudentsWithoutInteraction
    {
        private record Response(
            Guid Id,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            Guid? AllocationID,
            DateTime? LastLoginDate,
            string UserCode,
            string? Image
        );

        public sealed class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/StudentsWithoutInteraction",
                    ([FromQuery] string dateBefore,
                     [FromServices] SmartUniDbContext dbContext,
                     [FromServices] ILogger<Endpoint> logger,
                     CancellationToken cancellationToken) =>
                        HandleAsync(dateBefore, logger, dbContext, cancellationToken))
                    .RequireAuthorization("api")
                    .Produces<List<Response>>()
                    .Produces<NotFound>()
                    .WithTags(nameof(Report));
            }

            private static async Task<Results<Ok<List<Response>>, NotFound, BadRequest<string>>> HandleAsync(
    string dateBefore,
    ILogger<Endpoint> logger,
    SmartUniDbContext dbContext,
    CancellationToken cancellationToken)
            {
                logger.LogInformation("Fetching students who haven't interacted since {DateBefore}", dateBefore);

                if (!DateTime.TryParse(dateBefore, out var parsedDate))
                {
                    return TypedResults.BadRequest("Invalid date format. Please use yyyy-MM-dd.");
                }

                // Force UTC to avoid PostgreSQL error
                var cutoffDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

                var students = await dbContext.Student
                    .Include(s => s.Identity)
                    .Include(s => s.Allocation)
                    .Where(s =>
                        !s.IsDeleted &&
                        s.Identity.LastLoginDate != null &&
                        s.Identity.LastLoginDate < cutoffDate)
                    .Select(s => new Response(
                        s.Id,
                        s.Name,
                        s.Identity.Email,
                        s.Identity.PhoneNumber,
                        s.Gender.ToString(),
                        s.Major.ToString(),
                        s.Allocation != null && !s.Allocation.IsDeleted ? s.Allocation.Id : null,
                        s.Identity.LastLoginDate,
                    s.UserCode,
                        Convert.ToBase64String(s.Image)
                    ))
                    .ToListAsync(cancellationToken);

                if (!students.Any())
                {
                    logger.LogWarning("No students found with last login before {Date}", cutoffDate);
                    return TypedResults.NotFound();
                }

                logger.LogInformation("Retrieved {Count} students with last login before {Date}", students.Count, cutoffDate);
                return TypedResults.Ok(students);
            }

        }
    }
}
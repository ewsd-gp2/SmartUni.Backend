using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Dashboard.Queries
{
    public class GetStudentDashBoard
    {
        private sealed record Response(
            Guid StudentId,
            string Name,
            string Email,
            string PhoneNumber,
            string Gender,
            string Major,
            string Profile,
            AllocationResponse? allocation,
            List<NotificationResponse> Notifications);

        private record NotificationResponse(
            Guid BlogId,
            string Name,
            string Avatar,
            DateTime CreatedOn,
            Enums.NotificationType NotificationType);

        private sealed record AllocationResponse(Guid AllocationId, Guid TutorId, string Name, string Profile);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/dashboard/student/{id:guid}/",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .WithDescription("Get dashboard details for a student")
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
                logger.LogInformation("Fetching details for student with ID: {Id}", id);

                Student.Student? student = await dbContext.Student
                    .Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (student is null)
                {
                    logger.LogInformation("Student with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                // Get allocation and tutor (if any)
                var allocation = await dbContext.Allocation
                    .Where(a => a.StudentId == id)
                    .Select(a => new { a.Id, a.TutorId })
                    .FirstOrDefaultAsync(cancellationToken);

                AllocationResponse? allocationResponse = null;

                if (allocation != null)
                {
                    var tutor = await dbContext.Tutor
                        .Where(a => a.Id == allocation.TutorId)
                        .Select(a => new { a.Name })
                        .FirstOrDefaultAsync(cancellationToken);

                    allocationResponse = new AllocationResponse(
                        allocation.Id,
                        allocation.TutorId,
                        tutor?.Name ?? "Unknown",
                        Convert.ToBase64String(student.Image ?? [])
                    );
                }

                List<NotificationResponse> notifications = [];
                notifications.AddRange(dbContext.Blog.Where(x => x.CreatedBy == id).Include(b => b.Reactions)
                    .OrderByDescending(x => x.CreatedOn)
                    .SelectMany(x => x.Reactions)
                    .OrderByDescending(x => x.ReactedOn)
                    .Select(x =>
                        new NotificationResponse(
                            x.BlogId,
                            UserHelper.GetUserNameByUserId(x.ReacterId, dbContext).Result,
                            Convert.ToBase64String(UserHelper.GetUserAvatarByUserId(x.ReacterId, dbContext).Result ??
                                                   Array.Empty<byte>()),
                            x.ReactedOn,
                            Enums.NotificationType.Reaction)));
                notifications.AddRange(dbContext.Blog.Where(x => x.CreatedBy == id).Include(b => b.Comments)
                    .OrderByDescending(x => x.CreatedOn)
                    .SelectMany(x => x.Comments)
                    .OrderByDescending(x => x.CommentedOn)
                    .Select(x => new NotificationResponse(x.BlogId,
                        UserHelper.GetUserNameByUserId(x.CommenterId, dbContext).Result,
                        Convert.ToBase64String(UserHelper.GetUserAvatarByUserId(x.CommenterId, dbContext).Result ??
                                               Array.Empty<byte>()),
                        x.CommentedOn,
                        Enums.NotificationType.Comment)));

                Response response = new(
                    student.Id,
                    student.Name,
                    student.Identity.Email!,
                    student.Identity.PhoneNumber!,
                    student.Gender.ToString(),
                    student.Major.ToString(),
                    Convert.ToBase64String(student.Image ?? []),
                    allocationResponse,
                    notifications
                );

                logger.LogInformation("Successfully fetched details for student with ID: {Id}", id);
                return TypedResults.Ok(response);
            }
        }
    }
}
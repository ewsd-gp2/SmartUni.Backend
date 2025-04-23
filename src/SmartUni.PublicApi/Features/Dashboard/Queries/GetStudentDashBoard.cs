using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Features.Blog;
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
            string NotificationType);

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
                List<Blog.Blog> blogs = await dbContext.Blog.Where(x => x.CreatedBy == student.IdentityId)
                    .Include(b =>
                        b.Reactions.Where(x => x.ReacterId != student.IdentityId).OrderByDescending(x => x.ReactedOn))
                    .Include(b =>
                        b.Comments.Where(x => x.CommenterId != student.IdentityId)
                            .OrderByDescending(x => x.CommentedOn))
                    .OrderByDescending(x => x.CreatedOn)
                    .ToListAsync(cancellationToken);

                foreach (BlogReaction reaction in blogs.SelectMany(x => x.Reactions))
                {
                    string name = await UserHelper.GetUserNameByUserId(reaction.ReacterId, dbContext);
                    string avatar =
                        Convert.ToBase64String(await UserHelper.GetUserAvatarByUserId(reaction.ReacterId, dbContext) ??
                                               []);
                    notifications.Add(new NotificationResponse(reaction.BlogId, name, avatar, reaction.ReactedOn,
                        nameof(Enums.NotificationType.Reaction)));
                }

                foreach (BlogComment comments in blogs.SelectMany(x => x.Comments))
                {
                    string name = await UserHelper.GetUserNameByUserId(comments.CommenterId, dbContext);
                    string avatar =
                        Convert.ToBase64String(
                            await UserHelper.GetUserAvatarByUserId(comments.CommenterId, dbContext) ??
                            []);
                    notifications.Add(new NotificationResponse(comments.BlogId, name, avatar, comments.CommentedOn,
                        nameof(Enums.NotificationType.Comment)));
                }

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
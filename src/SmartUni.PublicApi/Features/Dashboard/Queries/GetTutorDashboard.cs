using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
using SmartUni.PublicApi.Features.Blog;
using SmartUni.PublicApi.Persistence;
using Convert = System.Convert;

namespace SmartUni.PublicApi.Features.Dashboard.Queries
{
    public class GetTutorDashboard
    {
        private sealed record Response(
            Guid TutorId,
            string Name,
            string Email,
            string Avatar,
            string PhoneNumber,
            string Gender,
            string Major,
            List<AllocationResponse> Students,
            List<NotificationResponse> Notifications);

        private record NotificationResponse(
            Guid BlogId,
            string Name,
            string Avatar,
            DateTime CreatedOn,
            string NotificationType);

        private sealed record AllocationResponse(Guid AllocationId, Guid StudentId, string Name, string Avatar);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/dashboard/tutor/{id:guid}/",
                        ([FromRoute] Guid id, [FromServices] SmartUniDbContext dbContext,
                                [FromServices] ILogger<Endpoint> logger, CancellationToken cancellationToken) =>
                            HandleAsync(id, dbContext, logger, cancellationToken))
                    .WithDescription("Get dashboard details for a tutor")
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
                logger.LogInformation("Fetching details for tutor with ID: {Id}", id);

                Tutor.Tutor? tutor = await dbContext.Tutor.Include(x => x.Identity)
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (tutor is null)
                {
                    logger.LogInformation("Tutor with ID: {Id} not found", id);
                    return TypedResults.NotFound();
                }

                List<AllocationResponse> allocations = dbContext.Allocation.Where(a => a.TutorId == id && !a.IsDeleted)
                    .Include(a => a.Student)
                    .Select(a => new AllocationResponse(
                        a.Id,
                        a.StudentId,
                        a.Student.Name,
                        Convert.ToBase64String(a.Student.Image ?? Array.Empty<byte>()))
                    ).ToList();

                List<NotificationResponse> notifications = [];
                List<Blog.Blog> blogs = await dbContext.Blog.Where(x => x.CreatedBy == tutor.IdentityId)
                    .Include(b =>
                        b.Reactions.Where(x => x.ReacterId != tutor.IdentityId).OrderByDescending(x => x.ReactedOn))
                    .Include(b =>
                        b.Comments.Where(x => x.CommenterId != tutor.IdentityId).OrderByDescending(x => x.CommentedOn))
                    .OrderByDescending(x => x.CreatedOn)
                    .ToListAsync(cancellationToken);

                foreach (BlogReaction reaction in blogs.SelectMany(x => x.Reactions))
                {
                    string name = await UserHelper.GetUserNameByUserId(reaction.ReacterId, dbContext);
                    string avatar =
                        Convert.ToBase64String(await UserHelper.GetUserAvatarByUserId(reaction.ReacterId, dbContext) ??
                                               []);
                    notifications.Add(new NotificationResponse(reaction.Id, name, avatar, reaction.ReactedOn,
                        nameof(Enums.NotificationType.Reaction)));
                }

                foreach (BlogComment comments in blogs.SelectMany(x => x.Comments))
                {
                    string name = await UserHelper.GetUserNameByUserId(comments.CommenterId, dbContext);
                    string avatar =
                        Convert.ToBase64String(
                            await UserHelper.GetUserAvatarByUserId(comments.CommenterId, dbContext) ??
                            []);
                    notifications.Add(new NotificationResponse(comments.Id, name, avatar, comments.CommentedOn,
                        nameof(Enums.NotificationType.Comment)));
                }

                Response response = new(
                    tutor!.Id,
                    tutor.Name,
                    tutor.Identity.Email!,
                    Convert.ToBase64String(tutor.Image ?? []),
                    tutor.Identity.PhoneNumber!,
                    tutor.Gender.ToString(),
                    tutor.Major.ToString(),
                    allocations,
                    notifications
                );
                logger.LogInformation("Successfully fetched details for tutor with ID: {Id} with response: {Response}",
                    id, response);
                return TypedResults.Ok(response);
            }
        }
    }
}
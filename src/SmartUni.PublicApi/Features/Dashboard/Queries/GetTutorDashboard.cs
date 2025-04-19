using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Common.Helpers;
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
            Enums.NotificationType NotificationType);

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

                List<AllocationResponse> allocations = dbContext.Allocation.Where(a => a.TutorId == id)
                    .Include(a => a.Student)
                    .Select(a => new AllocationResponse(
                        a.Id,
                        a.StudentId,
                        a.Student.Name,
                        Convert.ToBase64String(tutor.Image ?? Array.Empty<byte>()))
                    ).ToList();

                List<NotificationResponse> notifications = [];
                notifications.AddRange(dbContext.Blog.Where(x => x.CreatedBy == id).Include(b => b.Reactions)
                    .OrderByDescending(x => x.CreatedOn)
                    .SelectMany(x => x.Reactions)
                    .OrderByDescending(x => x.ReactedOn)
                    .Select(x => new NotificationResponse(x.BlogId,
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
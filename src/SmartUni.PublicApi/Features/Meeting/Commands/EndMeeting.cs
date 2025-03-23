using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Meeting.Commands
{
    public class EndMeeting
    {
        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/meeting/{meetingId:guid}/complete", Handle)
                    .RequireAuthorization("api")
                    .WithDescription("Complete a meeting")
                    .Produces<Ok>()
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Meeting));
            }

            private static async Task<IResult> Handle([FromRoute] Guid meetingId, ClaimsPrincipal claims,
                SmartUniDbContext dbContext, ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to cancel meeting with id: {MeetingId}", meetingId);

                Meeting? meeting = await dbContext.Meeting
                    .Include(x => x.Participants)
                    .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

                if (meeting == null)
                {
                    logger.LogInformation("Meeting with id: {MeetingId} not found", meetingId);
                    return Results.NotFound();
                }

                Guid updatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

                meeting.Status = Enums.MeetingStatus.Completed;
                meeting.UpdatedBy = updatedBy;
                meeting.UpdatedOn = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                return Results.Ok();
            }
        }
    }
}
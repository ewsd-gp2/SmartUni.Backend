using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Meeting.Commands
{
    public class MarkMeetingAttendance
    {
        private sealed record Request(Guid ParticipantId, Enums.AttendanceStatus Attendance);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPut("/meeting/{meetingId:guid}/attendance", Handle)
                    .RequireAuthorization("api")
                    .WithDescription("Mark attendance for meeting participants")
                    .Produces<Ok>()
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Meeting));
            }

            private static async Task<IResult> Handle([FromRoute] Guid meetingId, [FromBody] List<Request> request,
                ClaimsPrincipal claims,
                SmartUniDbContext context, ILogger<Endpoint> logger, CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to mark attendance for meeting with id: {MeetingId}", meetingId);

                Meeting? meeting = await context.Meeting
                    .Include(x => x.Participants)
                    .FirstOrDefaultAsync(x => x.Id == meetingId, cancellationToken);

                if (meeting is null)
                {
                    return Results.NotFound();
                }

                foreach ((Guid participantId, Enums.AttendanceStatus attendanceStatus) in request)
                {
                    MeetingParticipant? participant = meeting.Participants.FirstOrDefault(x => x.Id == participantId);
                    if (participant is null)
                    {
                        logger.LogWarning("Participant with id: {ParticipantId} not found", participantId);
                        return Results.NotFound(participantId);
                    }

                    participant.Attendance = attendanceStatus;
                }

                meeting.UpdatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
                meeting.UpdatedOn = DateTime.UtcNow;

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Attendance marked for meeting with id: {MeetingId}", meetingId);
                return Results.Ok();
            }
        }
    }
}
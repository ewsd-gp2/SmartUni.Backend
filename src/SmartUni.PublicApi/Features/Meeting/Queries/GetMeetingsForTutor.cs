using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Features.Meeting.Commands;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Meeting.Queries
{
    public class GetMeetingsForTutor
    {
        private sealed record Request(DateTime StartTime, DateTime EndTime);

        private sealed record Response(
            Guid TutorId,
            DateTime StartTime,
            DateTime EndTime,
            List<ParticipantResponse> Participants,
            string Status,
            string Title,
            bool IsOnline,
            string? Location,
            string? LinkType,
            string? Url,
            string? Agenda);

        private sealed record ParticipantResponse(
            Guid Id,
            Guid StudentId,
            string Name,
            string Email,
            string? Avatar,
            string Attendance,
            string? Note);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/meeting/tutor/{tutorId:guid}", Handle)
                    .RequireAuthorization("api")
                    .WithDescription("Get meetings for tutor in a date range")
                    .Accepts<Request>("application/json")
                    .Produces<Ok>()
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Meeting));
            }

            private static async Task<IResult> Handle([FromRoute] Guid tutorId,
                [FromBody] CreateMeeting.Request request,
                [FromServices] SmartUniDbContext dbContext,
                [FromServices] ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation(
                    "Submitted to get meetings for tutor with id: {TutorId} starting from {StartTime} to {EndTime}",
                    tutorId,
                    request.StartTime.ToString("dd MMM, yyyy HH:mm"),
                    request.EndTime.ToString("dd MMM yyyy HH:mm"));

                bool tutorExists = await dbContext.Tutor.AnyAsync(t => t.Id == tutorId, cancellationToken);

                if (!tutorExists)
                {
                    return Results.NotFound("Invalid tutor id");
                }

                List<Meeting> meetings = await dbContext.Meeting
                    .Where(m => m.OrganizerId == tutorId)
                    .Where(m => m.StartTime >= request.StartTime)
                    .Where(m => m.EndTime <= request.EndTime)
                    .Include(m => m.Participants)
                    .ThenInclude(mp => mp.Student)
                    .ToListAsync(cancellationToken);

                List<Response> response = [];
                foreach (Meeting meeting in meetings)
                {
                    IEnumerable<ParticipantResponse> participants =
                        meeting.Participants.Select(x =>
                            new ParticipantResponse(x.Id, x.StudentId, x.Student.Name, x.Student.Email, "",
                                x.Attendance.ToString(), x.Note)).ToList();

                    response.Add(new Response(meeting.OrganizerId, meeting.StartTime, meeting.EndTime,
                        participants.ToList(), meeting.Status.ToString(), meeting.Title, meeting.IsOnline,
                        meeting.Location,
                        meeting.LinkType.ToString(), meeting.Url, meeting.Agenda));
                }


                return Results.Ok(response);
            }
        }
    }
}
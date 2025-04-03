using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Features.Meeting.Commands;
using SmartUni.PublicApi.Persistence;

namespace SmartUni.PublicApi.Features.Meeting.Queries
{
    public class GetMeetingsForStudent
    {
        private sealed record Request(DateTime StartTime, DateTime EndTime);

        private sealed record Response(
            Guid AttendanceId,
            Guid MeetingId,
            Guid OrganizerId,
            string OrganizerName,
            DateTime StartTime,
            DateTime EndTime,
            string Status,
            string Title,
            bool IsOnline,
            string? Location,
            string? LinkType,
            string? Url,
            string? Agenda,
            string Attendance,
            string? Note);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/meeting/student/{studentId:guid}", Handle)
                    .RequireAuthorization("api")
                    .WithDescription("Create new meeting for tutor and students")
                    .Accepts<Request>("application/json")
                    .Produces<Ok>()
                    .Produces<NotFound>(404)
                    .WithTags(nameof(Meeting));
            }

            private static async Task<IResult> Handle([FromRoute] Guid studentId,
                [FromBody] Request request,
                [FromServices] SmartUniDbContext dbContext,
                [FromServices] ILogger<Endpoint> logger,
                CancellationToken cancellationToken)
            {
                logger.LogInformation(
                    "Submitted to get meetings for tutor with id: {StudentId} starting from {StartTime} to {EndTime}",
                    studentId,
                    request.StartTime.ToString("dd MMM, yyyy HH:mm"),
                    request.EndTime.ToString("dd MMM yyyy HH:mm"));

                bool studentExists = await dbContext.Student.AnyAsync(t => t.Id == studentId, cancellationToken);

                if (!studentExists)
                {
                    return Results.NotFound("Invalid student id");
                }

                List<Response> response = await dbContext.MeetingParticipants
                    .Where(x => x.StudentId == studentId)
                    .Include(p => p.Meeting)
                    .ThenInclude(m => m.Organizer)
                    .Select(x => new Response(
                        x.Id, 
                        x.MeetingId, 
                        x.Meeting.OrganizerId, 
                        x.Meeting.Organizer.Name,
                        x.Meeting.StartTime, 
                        x.Meeting.EndTime, 
                        x.Meeting.Status.ToString(), 
                        x.Meeting.Title,
                        x.Meeting.IsOnline,
                        x.Meeting.Location, 
                        x.Meeting.LinkType.ToString(), 
                        x.Meeting.Url, 
                        x.Meeting.Agenda,
                        x.Attendance.ToString(), x.Note))
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Found {Count} meetings for student with id: {StudentId}, response: {@Response}",
                    response.Count,
                    studentId, response);

                return Results.Ok(response);
            }
        }
    }
}
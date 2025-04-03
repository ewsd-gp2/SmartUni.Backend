using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartUni.PublicApi.Common.Domain;
using SmartUni.PublicApi.Persistence;
using System.Security.Claims;

namespace SmartUni.PublicApi.Features.Meeting.Commands
{
    public class CreateMeeting
    {
        public sealed record Request(
            DateTime StartTime,
            DateTime EndTime,
            Guid OrganizerId,
            List<Guid> Participants,
            string Title,
            bool IsOnline,
            string? Location,
            string LinkType,
            string? Url,
            string? Agenda);

        public class Endpoint : IEndpoint
        {
            public static void MapEndpoint(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapPost("/meeting", Handle)
                    .RequireAuthorization("api")
                    .WithDescription("Create new meeting for tutor and students")
                    .Accepts<Request>("application/json")
                    .Produces<Created>(201)
                    .Produces<BadRequest<List<ValidationFailure>>>(400)
                    .WithTags(nameof(Meeting));
            }

            private static async Task<IResult> Handle([FromBody] Request request, ClaimsPrincipal claims,
                ILogger<Endpoint> logger,
                SmartUniDbContext dbContext, CancellationToken cancellationToken)
            {
                logger.LogInformation("Submitted to create a new meeting with request: {Request}", request);

                ValidationResult? validationResult = await new Validator().ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Request failed validation with errors: {Errors}", validationResult.Errors);
                    return TypedResults.BadRequest(validationResult.Errors);
                }

                Meeting meeting = MapToDomain(request);
                meeting.CreatedOn = DateTime.UtcNow;
                meeting.CreatedBy = Guid.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                               throw new InvalidOperationException(ClaimTypes.NameIdentifier));

                dbContext.Meeting.Add(meeting);
                await dbContext.SaveChangesAsync(cancellationToken);

                return TypedResults.Created("/meeting/" + meeting.Id, new { meeting.Id });
            }

            private static Meeting MapToDomain(Request request)
            {
                Guid meetingId = Guid.NewGuid();
                List<MeetingParticipant> participants = [];
                participants.AddRange(request.Participants.Select(studentId =>
                    new MeetingParticipant { Id = Guid.NewGuid(), MeetingId = meetingId, StudentId = studentId }));

                Meeting meeting = new()
                {
                    Id = meetingId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    OrganizerId = request.OrganizerId,
                    Participants = participants,
                    Title = request.Title,
                    IsOnline = request.IsOnline,
                    Location = request.Location,
                    LinkType = Enum.Parse<Enums.MeetingLinkType>(request.LinkType), 
                    Url = request.Url,
                    Agenda = request.Agenda
                };

                return meeting;
            }
        }

        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
                RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
                RuleFor(x => x.OrganizerId).NotEmpty();
                RuleFor(x => x.Participants).NotEmpty();
                RuleFor(x => x.Title).NotEmpty();
                RuleFor(x => x.IsOnline).NotEmpty();
                RuleFor(x => x.LinkType).IsEnumName(typeof(Enums.MeetingLinkType), false);
            }
        }
    }
}
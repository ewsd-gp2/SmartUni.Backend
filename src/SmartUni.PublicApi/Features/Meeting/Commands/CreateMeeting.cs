using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Features.Meeting.Commands
{
    public class CreateMeeting
    {
        private sealed record Request(
            DateOnly Date,
            TimeOnly StartTime,
            TimeOnly EndTime,
            Guid OrganizerId,
            List<Guid> Participants,
            string MeetingTitle,
            bool IsMeetingOnline,
            string? MeetingLocation,
            Enums.MeetingLinkType? LinkType,
            string? MeetingUrl,
            string? Agenda);
    }
}
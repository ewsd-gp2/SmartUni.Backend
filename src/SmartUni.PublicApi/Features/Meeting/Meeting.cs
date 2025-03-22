using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartUni.PublicApi.Features.Meeting
{
    public class Meeting : BaseEntity
    {
        public Guid Id { get; set; }
        public string MeetingTitle { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Enums.MeetingStatus Status { get; set; }

        public Tutor.Tutor Organizer { get; set; }

        [ForeignKey(nameof(Organizer))] public Guid OrganizerId { get; set; }

        public List<MeetingParticipant> Participants { get; set; } = [];

        public bool IsMeetingOnline { get; set; }
        public string? MeetingLocation { get; set; }
        public Enums.MeetingLinkType? LinkType { get; set; }
        public string? MeetingUrl { get; set; }
        public string? Agenda { get; set; }
    }

    public class MeetingParticipant
    {
        public Guid Id { get; set; }
        public Student.Student Student { get; set; }
        public Guid StudentId { get; set; }
        public Enums.AttendanceStatus Attendance { get; set; }
    }
}
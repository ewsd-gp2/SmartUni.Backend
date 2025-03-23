using SmartUni.PublicApi.Common.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartUni.PublicApi.Features.Meeting
{
    public class Meeting : BaseEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Enums.MeetingStatus Status { get; set; } = Enums.MeetingStatus.New;

        public Tutor.Tutor Organizer { get; set; }

        [ForeignKey(nameof(Organizer))] public Guid OrganizerId { get; set; }

        public List<MeetingParticipant> Participants { get; set; } = [];

        public bool IsOnline { get; set; }
        public string? Location { get; set; }
        public Enums.MeetingLinkType? LinkType { get; set; }
        public string? Url { get; set; }
        public string? Agenda { get; set; }
    }

    public class MeetingParticipant
    {
        public Guid Id { get; set; }

        public Meeting Meeting { get; set; }

        [ForeignKey(nameof(Meeting))] public Guid MeetingId { get; set; }

        public Student.Student Student { get; set; }
        [ForeignKey(nameof(Student))] public Guid StudentId { get; set; }

        public Enums.AttendanceStatus Attendance { get; set; } = Enums.AttendanceStatus.Absent;
        public string? Note { get; set; }
    }
}
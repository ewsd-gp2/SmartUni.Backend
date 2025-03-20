using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Features.Meeting
{
    public class Meeting : BaseEntity
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Enums.MeetingStatus Status { get; set; }
        public Tutor.Tutor Organizer { get; set; }
        public Guid OrganizerId { get; set; }
        public List<Participant> Participants { get; set; } = [];
    }

    public class Participant
    {
        public Guid Id { get; set; }
        public Student.Student Student { get; set; }
        public Guid StudentId { get; set; }
        public Enums.AttendanceStatus Attendance { get; set; }
    }
}
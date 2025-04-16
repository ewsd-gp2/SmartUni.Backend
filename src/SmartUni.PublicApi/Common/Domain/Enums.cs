using System.ComponentModel;

namespace SmartUni.PublicApi.Common.Domain
{
    public static class Enums
    {
        public enum AttendanceStatus
        {
            Present,
            Leave,
            Absent
        }

        public enum GenderType
        {
            Male,
            Female
        }

        public enum MajorType
        {
            Computing,
            [Description("Information Systems")] InformationSystems,
            Networking
        }

        public enum MeetingLinkType
        {
            Zoom,
            [Description("Google Meet")] GoogleMeet,
            [Description("Microsoft Teams")] MicrosoftTeams
        }

        public enum MeetingStatus
        {
            New,
            Cancelled,
            Completed
        }

        public enum UserCodePrefix
        {
            Sta,
            Tut,
            Stu
        }
        public enum MostViewPage
        {
            Allocation,
            [Description("Student DashBoard")]StudentDashboard,
            [Description("Tutor DashBoard")]TutorDashboard,
            [Description("Student List")]StudentList,
            [Description("Tutor List")]TutorList,
            Chat,
            Blog,
            Meeting,
            Profile,
            [Description("Students Without Interaction")]StudentsWithoutAllocation,
            [Description("Students Without Tutors")]StudentsWithoutTutor,
            [Description("Total Message")] TotalMessage,
            [Description("Most Viewed Pages")]MostViewedPages,
            [Description("Chatting WIth AI")]ChattingWithAI,
        }
        public enum SenderType
        {
            Student,
            Tutor
        }
    }
}
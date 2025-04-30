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

        public enum BlogType
        {
            [Description("News Letter")] NewsLetter,
            [Description("Knowledge Sharing")] KnowledgeSharing,
            Announcement
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

        public enum MostViewPage
        {
            Allocation,
            [Description("Student DashBoard")] StudentDashboard,
            [Description("Tutor DashBoard")] TutorDashboard,
            [Description("Student List")] StudentList,
            [Description("Tutor List")] TutorList,
            Chat,
            Blog,
            Meeting,
            Profile,

            [Description("Students Without Interaction")]
            StudentsWithoutAllocation,

            [Description("Students Without Tutors")]
            StudentsWithoutTutor,
            [Description("Total Message")] TotalMessage,
            [Description("Most Viewed Pages")] MostViewedPages,
            [Description("Chatting WIth AI")] ChattingWithAI
        }

        public enum NotificationType
        {
            Reaction,
            Comment
        }

        public enum RoleType
        {
            Staff,
            Tutor,
            Student,
            AuthorizedStaff
        }

        public enum SenderType
        {
            Student,
            Tutor
        }

        public enum UserCodePrefix
        {
            Sta,
            Tut,
            Stu
        }
    }
}
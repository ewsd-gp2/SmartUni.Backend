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
    }
}
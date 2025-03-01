using System.ComponentModel;

namespace SmartUni.PublicApi.Common.Domain
{
    public static class Enums
    {
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
    }
}
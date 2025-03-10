using SmartUni.PublicApi.Common.Domain;

namespace SmartUni.PublicApi.Common.Helpers
{
    public class UserCodeHelpers
    {
        public static string GenerateUserCode(Enums.UserCodePrefix prefix, string name, string email)
        {
            return $"{prefix.ToString().ToLower()}-{email[.. email.IndexOf('@')]}{name.Length}";
        }
    }
}
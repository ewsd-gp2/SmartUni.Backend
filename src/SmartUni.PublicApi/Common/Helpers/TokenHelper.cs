using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SmartUni.PublicApi.Common.Domain;
using System.Security.Claims;

namespace SmartUni.PublicApi.Common.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateToken(BaseUser user)
        {
            byte[] key = "DoNotShareThisSuperSecretKey!@SDF123!@#"u8.ToArray();
            JsonWebTokenHandler tokenHandler = new();
            int inactiveDays = user.LastActiveDate == null
                ? 0
                : DateTime.Now.Date.Subtract((DateTime)user.LastActiveDate).Days;

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Tutor.Id.ToString()),
                new(JwtRegisteredClaimNames.Name, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new("last_active_date",
                    user.LastActiveDate is null
                        ? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                        : new DateTimeOffset((DateTime)user.LastActiveDate).ToUnixTimeSeconds().ToString()),
                new("inactive_days", inactiveDays.ToString())
            ];

            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(3),
                Issuer = "http://localhost:7142",
                Audience = "http://localhost:5173",
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            string? token = tokenHandler.CreateToken(securityTokenDescriptor);
            return token;
        }
    }
}
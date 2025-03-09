using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace SmartUni.PublicApi.Common.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateToken(Guid identityId, Guid userId, string name, string email,
            DateTime? lastActiveDate, string role)
        {
            byte[] key = "donotsharethissupersecretkey"u8.ToArray();
            JsonWebTokenHandler tokenHandler = new();
            int inactiveDays = lastActiveDate == null ? 0 : DateTime.Now.Date.Subtract((DateTime)lastActiveDate).Days;

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, identityId.ToString()),
                new(JwtRegisteredClaimNames.Name, name),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new(ClaimTypes.Role, role),
                new("last_active_date",
                    lastActiveDate is null
                        ? DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                        : new DateTimeOffset((DateTime)lastActiveDate).ToUnixTimeSeconds().ToString()),
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
            return tokenHandler.CreateToken(token);
        }
    }
}
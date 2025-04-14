using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            string userId = user.Role switch
            {
                Enums.RoleType.Tutor => user.Tutor.Id.ToString(),
                Enums.RoleType.Staff => user.Staff.Id.ToString(),
                Enums.RoleType.Student => user.Student.Id.ToString(),
                _ => string.Empty
            };

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Name, user.UserName!),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new("role", user.Role.ToString()),
                new("identityId", user.Id.ToString()),
                new("isFirstLogin", user.LastLoginDate is null ? true.ToString() : false.ToString())
            ];

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme),
                Expires = DateTime.Now.AddHours(3),
                Issuer = "http://localhost:7142",
                Audience = "http://localhost:5173",
                NotBefore = DateTime.UtcNow.AddHours(-1),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            string? token = tokenHandler.CreateToken(tokenDescriptor);
            return token;
        }

        public static void SetTokensInsideCookie(string token, HttpContext context)
        {
            context.Response.Cookies.Append("accessToken", token,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddHours(2),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                });
        }
    }
}
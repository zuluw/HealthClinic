using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InnoClinic.Auth.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace InnoClinic.Auth.Application.Utils
{
    public static class JwtProvider
    {
        public static string GenerateAccessToken(User user, string key, string issuer, string audience)
        {
            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserStatus", user.Status.ToString()),
        };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rand = RandomNumberGenerator.Create();
            rand.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static Guid? GetUserIdFromToken(string token, string key, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateLifetime = false,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
                }, out _);

                var claim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(claim, out var userId) ? userId : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
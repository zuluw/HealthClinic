using FluentValidation;
using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InnoClinic.Auth.Domain.Exceptions;

namespace InnoClinic.Auth.Application.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration config, IValidator<RegisterDTO> registerValidator) : IAuthService
    {
        public async Task<string> RegisterAsync(RegisterDTO registerDTO)
        {
            var validationResult = await registerValidator.ValidateAsync(registerDTO);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            bool userExists = await userRepository.ExistsByUserNameAsync(registerDTO.UserName);
            if (userExists)
            {
                throw new BadRequestException("User with the same username already exists");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = registerDTO.UserName,
                FirstName = registerDTO.FirstName,
                LastName = registerDTO.LastName,
                Email = registerDTO.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
                CreatedAt = DateTime.UtcNow
            };

            userRepository.Add(newUser);
            await userRepository.SaveChangesAsync();
            return "Account successfully registered";
        }

        public async Task<object> LoginAsync(LoginDTO loginDTO)
        {
            var user = await userRepository.GetByUserNameAsync(loginDTO.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                throw new UnauthorizedException("Incorrect username or password");
            }

            return await CreateAuthResponse(user, "Login successful");
        }

        public async Task<object> RefreshAsync(JwtDTO jwtDTO)
        {
            var userId = GetUserId(jwtDTO.AccessToken);
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null || user.RefreshToken != jwtDTO.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new UnauthorizedException("Invalid or expired refresh token");
            }                

            return await CreateAuthResponse(user);
        }

        public async Task<object> GoogleAuthAsync(string email, string firstName, string lastName)
        {
            var user = await userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PasswordHash = "google-auth",
                    Status = UserStatus.Confirmed
                };
                userRepository.Add(user);
            }

            return await CreateAuthResponse(user, "Login via Google was successful");
        }

        private async Task<object> CreateAuthResponse(User user, string message = null)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.SaveChangesAsync();

            var response = new
            {
                Message = message,
                user.UserName,
                accessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                refreshToken
            };
            return response;
        }

        private JwtSecurityToken GenerateAccessToken(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("UserStatus", user.Status.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

            return new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rand = RandomNumberGenerator.Create())
            {
                rand.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        private Guid GetUserId(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                ValidAlgorithms = ["HS256"]
            }, out _);

            if (!Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                throw new UnauthorizedException("Invalid token payload");
            }

            return userId;
        }
    }
}
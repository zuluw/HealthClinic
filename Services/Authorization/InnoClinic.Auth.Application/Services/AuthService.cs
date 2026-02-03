using FluentValidation;
using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Application.Utils;
using InnoClinic.Auth.Domain.Exceptions;
using InnoClinic.Auth.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace InnoClinic.Auth.Application.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration config, IValidator<RegisterDTO> registerValidator) : IAuthService
    {
        public async Task<string> RegisterAsync(RegisterDTO data)
        {
            var validationResult = await registerValidator.ValidateAsync(data);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            bool userExists = await userRepository.ExistsByUserNameAsync(data.UserName);
            if (userExists)
            {
                throw new BadRequestException("User with the same username already exists");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = data.UserName,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PasswordHash = PasswordHasher.HashPassword(data.Password),
                CreatedAt = DateTime.UtcNow
            };

            userRepository.Add(newUser);
            await userRepository.SaveChangesAsync();
            return "Account successfully registered";
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO data)
        {
            var user = await userRepository.GetByUserNameAsync(data.UserName);
            
            if ((user == null) || (!PasswordHasher.VerifyPassword(data.Password, user.PasswordHash)))
            {
                throw new UnauthorizedException("Incorrect username or password");
            }

            return await CreateAuthResponse(user, "Login successful");
        }

        public async Task<AuthResponseDTO> RefreshAsync(JwtDTO data)
        {
            var userId = JwtProvider.GetUserIdFromToken(data.AccessToken, config["Jwt:Key"]!, config["Jwt:Issuer"]!, config["Jwt:Audience"]!) ?? throw new UnauthorizedException("Invalid token");
            var user = await userRepository.GetByIdAsync(userId);

            if ((user == null) || (user.RefreshToken != data.RefreshToken) || (user.RefreshTokenExpiryTime < DateTime.UtcNow))
            {
                throw new UnauthorizedException("Invalid or expired refresh token");
            }

            return await CreateAuthResponse(user);
        }

        public async Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName)
        {
            var user = await userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
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

        public async Task ChangeStatusAsync(Guid userId, UserStatus newStatus)
        {
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            user.Status = newStatus;

            await userRepository.SaveChangesAsync();
        }

        private async Task<AuthResponseDTO> CreateAuthResponse(User user, string? message = null)
        {
            var accessToken = JwtProvider.GenerateAccessToken(user, config["Jwt:Key"]!, config["Jwt:Issuer"]!, config["Jwt:Audience"]!);
            var refreshToken = JwtProvider.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.SaveChangesAsync();

            return new AuthResponseDTO(message, user.UserName, accessToken, refreshToken);
        }
    }
}
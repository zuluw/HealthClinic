using FluentValidation;
using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Application.Utils;
using InnoClinic.Auth.Domain.Exceptions;
using InnoClinic.Auth.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Auth.Application.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration config, IValidator<RegisterDTO> registerValidator, ILogger<AuthService> logger) : IAuthService
    {
        public async Task<bool> RegisterAsync(RegisterDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to register user: {UserName}", data.UserName);

            var validationResult = await registerValidator.ValidateAsync(data);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for user registration: {UserName}", data.UserName);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            bool userExists = await userRepository.ExistsByUserNameAsync(data.UserName, ct);
            if (userExists)
            {
                logger.LogWarning("Registration failed. User already exists: {UserName}", data.UserName);
                throw new BadRequestException("User with the same username already exists");
            }

            var newUser = new User
            {
                UserName = data.UserName,
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PasswordHash = PasswordHasher.HashPassword(data.Password),
            };

            userRepository.Add(newUser);
            await userRepository.SaveChangesAsync(ct);

            logger.LogInformation("User {UserName} successfully registered", data.UserName);
            return true;
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO data, CancellationToken ct)
        {
            logger.LogInformation("Login attempt for user: {UserName}", data.UserName);

            var user = await userRepository.GetByUserNameAsync(data.UserName, ct);
            
            if ((user == null) || (!PasswordHasher.VerifyPassword(data.Password, user.PasswordHash)))
            {
                logger.LogWarning("Invalid login attempt for user: {UserName}", data.UserName);
                throw new UnauthorizedException("Incorrect username or password");
            }

            return await CreateAuthResponse(user, ct, "Login successful");
        }

        public async Task<AuthResponseDTO> RefreshAsync(JwtDTO data, CancellationToken ct)
        {
            logger.LogInformation("Refreshing tokens for AccessToken: {Token}...", data.AccessToken[..10]);

            var userId = JwtProvider.GetUserIdFromToken(data.AccessToken, config["Jwt:Key"]!, config["Jwt:Issuer"]!, config["Jwt:Audience"]!) ?? throw new UnauthorizedException("Invalid token");
            var user = await userRepository.GetByIdAsync(userId, ct);

            if ((user == null) || (user.RefreshToken != data.RefreshToken) || (user.RefreshTokenExpiryTime < DateTime.UtcNow))
            {
                logger.LogWarning("Refresh token validation failed for UserID: {UserId}", userId);
                throw new UnauthorizedException("Invalid or expired refresh token");
            }

            return await CreateAuthResponse(user, ct);
        }

        public async Task<AuthResponseDTO> GoogleAuthAsync(string email, string firstName, string lastName, CancellationToken ct)
        {
            var user = await userRepository.GetByEmailAsync(email, ct);

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

            return await CreateAuthResponse(user, ct, "Login via Google was successful");
        }

        public async Task ChangeStatusAsync(Guid userId, UserStatus newStatus, CancellationToken ct)
        {
            logger.LogInformation("Changing status for UserID: {UserId} to {Status}", userId, newStatus);
      
            var user = await userRepository.GetByIdAsync(userId, ct);

            if (user == null)
            {
                logger.LogError("Status change failed. User {UserId} not found", userId);
                throw new NotFoundException("User not found");
            }

            user.Status = newStatus;
            await userRepository.SaveChangesAsync(ct);

            logger.LogInformation("Status for UserID: {UserId} updated to {Status}", userId, newStatus);
        }

        private async Task<AuthResponseDTO> CreateAuthResponse(User user, CancellationToken ct, string message = "")
        {
            var accessToken = JwtProvider.GenerateAccessToken(user, config["Jwt:Key"]!, config["Jwt:Issuer"]!, config["Jwt:Audience"]!);
            var refreshToken = JwtProvider.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await userRepository.SaveChangesAsync(ct);

            return new AuthResponseDTO(user.Id, user.UserName, accessToken, refreshToken, message);
        }
    }
}
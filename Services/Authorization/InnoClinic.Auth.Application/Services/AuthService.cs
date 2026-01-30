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

namespace InnoClinic.Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;
    private readonly IValidator<RegisterDTO> _registerValidator;

    public AuthService(IUserRepository userRepository, IConfiguration config, IValidator<RegisterDTO> registerValidator)
    {
        _userRepository = userRepository;
        _config = config;
        _registerValidator = registerValidator;
    }

    public async Task<string> RegisterAsync(RegisterDTO registerDTO)
    {
        var validationResult = await _registerValidator.ValidateAsync(registerDTO);
        if (!validationResult.IsValid)
            throw new Exception(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        bool userExists = await _userRepository.ExistsByUserNameAsync(registerDTO.UserName);
        if (userExists) 
            throw new Exception("User with the same username is exists!");

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

        _userRepository.Add(newUser);
        await _userRepository.SaveChangesAsync();
        return "Account successfully registered!";
    }

    public async Task<object> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _userRepository.GetByUserNameAsync(loginDTO.UserName); 
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Incorrect username or password");

        return await CreateAuthResponse(user, "Login successful!");
    }

    public async Task<object> RefreshAsync(JwtDTO jwtDTO)
    {
        var userId = GetUserId(jwtDTO.AccessToken);
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || user.RefreshToken != jwtDTO.RefreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new SecurityTokenException("Invalid or expired token");

        return await CreateAuthResponse(user);
    }

    public async Task<object> GoogleAuthAsync(string email, string firstName, string lastName)
    {
        var user = await _userRepository.GetByEmailAsync(email);

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
            _userRepository.Add(user);
        }

        return await CreateAuthResponse(user, "Login via Google was successful.");
    }

    private async Task<object> CreateAuthResponse(User user, string message = null)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync();

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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        
        return new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        
        using (var rand = RandomNumberGenerator.Create())
            rand.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
    }

    private Guid GetUserId(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            ValidAlgorithms = ["HS256"]
        }, out _);

        if (!Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            throw new SecurityTokenException("Invalid Id!");

        return userId;
    }
}
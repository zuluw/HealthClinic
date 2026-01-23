using FluentValidation;
using InnoClinic.Auth.Api.Data;
using InnoClinic.Auth.Api.DTOs;
using InnoClinic.Auth.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InnoClinic.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IValidator<RegisterDTO> _registerValidator;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IValidator<RegisterDTO> registerValidator, IConfiguration config)
    {
        _context = context;
        _registerValidator = registerValidator;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
        var validationResult = await _registerValidator.ValidateAsync(registerDTO);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
        }

        bool userExists = await _context.Users.AnyAsync(u => u.UserName == registerDTO.UserName);
        if (userExists)
            return Conflict("User with the same username is exists!");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = registerDTO.UserName,
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName,
            Email = registerDTO.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return Ok("Account successfully registered!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDTO loginDTO) 
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == loginDTO.UserName);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
        {
            return Unauthorized("Incorrect username or password");
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Message = "Login successful!",
            user.UserName,
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return BadRequest("Google authentication error!");

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                UserName = email!,
                Email = email!,
                FirstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty,
                PasswordHash = "google-auth",
                Status = UserStatus.Confirmed
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            Message = "Login via Google was successful.",
            token = new JwtSecurityTokenHandler().WriteToken(token),
        });
    }

    public JwtSecurityToken GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserStatus", user.Status.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return token;
    }
}
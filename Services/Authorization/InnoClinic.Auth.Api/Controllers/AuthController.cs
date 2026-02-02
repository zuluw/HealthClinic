using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InnoClinic.Auth.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var result = await _authService.RegisterAsync(registerDTO);
        return Ok(result);
        //return Created("api/auth/profile", result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var result = await _authService.LoginAsync(loginDTO);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] JwtDTO jwtDTO)
    {
        var result = await _authService.RefreshAsync(jwtDTO);
        return Ok(result);
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
        if (!result.Succeeded) return BadRequest("Google authentication error!");

        var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var firstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
        var lastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;

        var authResponse = await _authService.GoogleAuthAsync(email!, firstName, lastName);
        return Ok(authResponse);
    }
}
using InnoClinic.Auth.Application.DTOs;
using InnoClinic.Auth.Application.Interfaces;
using InnoClinic.Auth.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InnoClinic.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO data, CancellationToken ct)
        {
            var result = await authService.RegisterAsync(data, ct);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO data, CancellationToken ct)
        {
            var result = await authService.LoginAsync(data, ct);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] JwtDTO data, CancellationToken ct)
        {
            var result = await authService.RefreshAsync(data, ct);
            return Ok(result);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleResponse)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(CancellationToken ct)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return BadRequest("Google authentication error!");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = result.Principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            var lastName = result.Principal.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;

            var authResponse = await authService.GoogleAuthAsync(email!, firstName, lastName, ct);
            return Ok(authResponse);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] UserStatus newStatus, CancellationToken ct)
        {
            await authService.ChangeStatusAsync(id, newStatus, ct);
            return NoContent();
        }
    }
}
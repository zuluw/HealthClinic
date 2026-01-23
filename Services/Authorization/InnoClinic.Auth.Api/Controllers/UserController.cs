using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        [HttpGet("about")]
        public IActionResult About()
        {
            return Ok("About information.");
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("Profile information.");
        }

        [Authorize(Policy = "ConfirmedOnly")]
        [HttpPost("appointment")]
        public IActionResult Create()
        {
            return Ok("Created.");
        }
    }
}

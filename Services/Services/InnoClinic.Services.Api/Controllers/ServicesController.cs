using InnoClinic.Services.Application.Interfaces;
using InnoClinic.Services.Application.Services;
using InnoClinic.Services.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Services.Api.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController(IClinicService clinicService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetServices([FromQuery] ServiceCategory category, CancellationToken ct)
        {
            var result = await clinicService.GetServicesAsync(category, ct);
            return Ok(result);
        }
    }
}
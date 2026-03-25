using InnoClinic.Services.Application.DTOs;
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

        [HttpPost("specializations")]
        public async Task<IActionResult> CreateSpecialization([FromBody] SpecializationRequestDTO data, CancellationToken ct)
        {
            var result = await clinicService.CreateSpecializationAsync(data, ct);
            return StatusCode(201, result);
            //return CreatedAtAction(nameof(GetServices), new { result }, result);
        }

        [HttpPatch("specializations/{id}/status")]
        public async Task<IActionResult> UpdateSpecializationStatus(Guid id, [FromBody] bool isActive, CancellationToken ct)
        {
            await clinicService.UpdateSpecializationStatusAsync(id, isActive, ct);
            return NoContent();
        }
    }
}
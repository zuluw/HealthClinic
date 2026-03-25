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
            return CreatedAtAction(nameof(GetSpecializationById), new { id = result }, result);
        }

        [HttpPatch("specializations/{id}/status")]
        public async Task<IActionResult> UpdateSpecializationStatus(Guid id, [FromBody] bool isActive, CancellationToken ct)
        {
            await clinicService.UpdateSpecializationStatusAsync(id, isActive, ct);
            return NoContent();
        }

        [HttpPut("specializations/{id}")]
        public async Task<IActionResult> UpdateSpecialization(Guid id, [FromBody] UpdateSpecializationRequestDTO data, CancellationToken ct)
        {
            await clinicService.UpdateSpecializationAsync(id, data, ct);
            return NoContent();
        }

        [HttpGet("specializations/{id}")]
        public async Task<ActionResult<SpecializationResponseDTO>> GetSpecializationById(Guid id, CancellationToken ct)
        {
            var result = await clinicService.GetSpecializationByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("specializations")]
        public async Task<ActionResult<List<SpecializationListDTO>>> GetAllSpecializations(CancellationToken ct)
        {
            var result = await clinicService.GetAllSpecializationsAsync(ct);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateServiceStatus(Guid id, [FromBody] bool isActive, CancellationToken ct)
        {
            await clinicService.UpdateServiceStatusAsync(id, isActive, ct);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceRequestDTO data, CancellationToken ct)
        {
            await clinicService.UpdateServiceAsync(id, data, ct);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponseDTO>> GetServiceById(Guid id, CancellationToken ct)
        {
            var result = await clinicService.GetServiceByIdAsync(id, ct);
            return Ok(result);
        }
    }
}
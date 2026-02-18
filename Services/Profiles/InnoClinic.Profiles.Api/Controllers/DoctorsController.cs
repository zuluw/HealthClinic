using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Profiles.Api.Controllers
{
    [ApiController]
    [Route("api/profiles/doctors")]
    public class DoctorsController(IDoctorService doctorService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorRequestDTO data, CancellationToken ct)
        {
            var result = await doctorService.CreateAsync(data, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await doctorService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("account/{accountId:guid}")]
        public async Task<IActionResult> GetByAccountId(Guid accountId, CancellationToken ct)
        {
            var result = await doctorService.GetByAccountIdAsync(accountId, ct);
            return Ok(result);
        }

        [HttpGet("specialization/{specializationId:guid}")]
        public async Task<IActionResult> GetBySpecialization(Guid specializationId, CancellationToken ct)
        {
            var result = await doctorService.GetBySpecializationAsync(specializationId, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await doctorService.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DoctorRequestDTO data, CancellationToken ct)
        {
            await doctorService.UpdateAsync(id, data, ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await doctorService.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] DoctorStatus newStatus, CancellationToken ct)
        {
            await doctorService.ChangeStatusAsync(id, newStatus, ct);
            return NoContent();
        }
    }
}
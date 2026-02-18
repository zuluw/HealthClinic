using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Profiles.Api.Controllers
{
    [ApiController]
    [Route("api/profiles/patients")]
    public class PatientsController(IPatientService patientService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PatientRequestDTO data, CancellationToken ct)
        {
            var result = await patientService.CreateAsync(data, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var result = await patientService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("account/{accountId:guid}")]
        public async Task<IActionResult> GetByAccountId(Guid accountId, CancellationToken ct)
        {
            var result = await patientService.GetByAccountIdAsync(accountId, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await patientService.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatientRequestDTO data, CancellationToken ct)
        {
            await patientService.UpdateAsync(id, data, ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await patientService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
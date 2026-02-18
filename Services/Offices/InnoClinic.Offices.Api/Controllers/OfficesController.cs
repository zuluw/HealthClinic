using InnoClinic.Offices.Application.DTOs;
using InnoClinic.Offices.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Offices.Api.Controllers
{
    [ApiController]
    [Route("api/offices")]
    public class OfficesController(IOfficeService officeService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParameters parameters, CancellationToken ct)
        {
            var validParams = parameters with { PageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber };
            var result = await officeService.GetAllAsync(validParams, ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            var office = await officeService.GetByIdAsync(id, ct);
            if (office == null)
            {
                return NotFound();
            }

            return Ok(office);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OfficeRequestDTO officeDto, CancellationToken ct)
        {
            var result = await officeService.CreateAsync(officeDto, ct);

            return Created($"api/offices/{result.Id}", result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] OfficeRequestDTO officeDto, CancellationToken ct)
        {
            await officeService.UpdateAsync(id, officeDto, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
        {
            await officeService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
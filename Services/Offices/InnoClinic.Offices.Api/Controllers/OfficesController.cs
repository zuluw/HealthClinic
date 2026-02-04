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
        public async Task<IActionResult> GetAll([FromQuery] PaginationParameters parameters)
        {
            var validParams = parameters with { PageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber };
            var result = await officeService.GetAllAsync(validParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var office = await officeService.GetByIdAsync(id);
            if (office == null)
            {
                return NotFound();
            }

            return Ok(office);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OfficeDto officeDto)
        {
            var result = await officeService.CreateAsync(officeDto);

            return Created($"api/offices/{result.Id}", result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] OfficeDto officeDto)
        {
            await officeService.UpdateAsync(id, officeDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await officeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
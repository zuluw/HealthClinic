using InnoClinic.Appointments.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Appointments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
    {
        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations([FromQuery] string? officeId, CancellationToken ct)
        {
            var result = await appointmentService.GetSpecializationsAsync(officeId, ct);
            return Ok(result);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors([FromQuery] Guid? specializationId, [FromQuery] string? officeId, [FromQuery] Guid? serviceId, CancellationToken ct)
        {
            var result = await appointmentService.GetDoctorsAsync(specializationId, officeId, serviceId, ct);
            return Ok(result);
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices([FromQuery] Guid? specializationId, [FromQuery] Guid? doctorId, [FromQuery] string? officeId, CancellationToken ct)
        {
            var result = await appointmentService.GetServicesAsync(specializationId, doctorId, officeId, ct);
            return Ok(result);
        }

        [HttpGet("offices")]
        public async Task<IActionResult> GetOffices([FromQuery] Guid? specializationId, CancellationToken ct)
        {
            var result = await appointmentService.GetOfficesAsync(specializationId, ct);
            return Ok(result);
        }
    }
}
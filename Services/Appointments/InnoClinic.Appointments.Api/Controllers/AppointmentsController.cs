using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Appointments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController(IAppointmentService appointmentService, ITimeSlotService timeSlotService) : ControllerBase
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

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequestDTO request, CancellationToken ct)
        {
            var result = await appointmentService.CreateAppointmentAsync(request, ct);

            return Ok(result);
        }

        [HttpGet("doctor/{doctorId}/schedule")]
        public async Task<IActionResult> GetDoctorSchedule(Guid doctorId, [FromQuery] DateTime date, CancellationToken ct)
        {
            var targetDate = date == default ? DateTime.UtcNow.Date : date.Date;
            targetDate = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);

            var result = await appointmentService.GetDoctorScheduleAsync(doctorId, targetDate, ct);
            return Ok(result);
        }

        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetMySchedule([FromQuery] DateTime date, CancellationToken ct)
        {
            var targetDate = date == default ? DateTime.UtcNow : date;
            targetDate = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);

            var currentDoctorId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var result = await appointmentService.GetDoctorScheduleAsync(currentDoctorId, targetDate, ct);
            return Ok(result);
        }

        [HttpGet("receptionist")]
        public async Task<IActionResult> GetReceptionistAppointments([FromQuery] DateTime? date, [FromQuery] string? doctorName, [FromQuery] string? serviceName, [FromQuery] bool? isApproved, [FromQuery] string? officeId, CancellationToken ct)
        {
            var result = await appointmentService.GetReceptionistAppointmentsAsync(date, doctorName, serviceName, isApproved, officeId, ct);
            return Ok(result);
        }

        [HttpPatch("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(Guid id, [FromQuery] DateTime newDateTime, CancellationToken ct)
        {
            await appointmentService.RescheduleAppointmentAsync(id, newDateTime, ct);
            return NoContent();
        }

        [HttpPatch("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
        {
            await appointmentService.ApproveAppointmentAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
        {
            await appointmentService.CancelAppointmentAsync(id, ct);
            return NoContent();
        }

        [HttpGet("patient/{patientId}/history")]
        public async Task<IActionResult> GetPatientHistory(Guid patientId, CancellationToken ct)
        {
            var result = await appointmentService.GetPatientHistoryAsync(patientId, ct);
            return Ok(result);
        }

        [HttpPost("result")]
        public async Task<IActionResult> CreateResult([FromForm] ResultRequestDTO request, CancellationToken ct)
        {
            var result = await appointmentService.CreateResultAsync(request, ct);
            return Ok(result);
        }

        [HttpPut("results/{id}")]
        public async Task<IActionResult> UpdateResult(Guid id, [FromBody] UpdateResultRequestDTO request, CancellationToken ct)
        {
            var result = await appointmentService.UpdateResultAsync(id, request, ct);
            return Ok(result);
        }

        [HttpGet("{appointmentId}/result")]
        public async Task<IActionResult> GetResultByAppointment(Guid appointmentId, CancellationToken ct)
        {
            var result = await appointmentService.GetResultByAppointmentIdAsync(appointmentId, ct);
            return Ok(result);
        }

        [HttpGet("{appointmentId}/result/pdf")]
        public async Task<IActionResult> DownloadResultPdf(Guid appointmentId, CancellationToken ct)
        {
            var pdfResult = await appointmentService.GenerateResultPdfAsync(appointmentId, ct);

            return File(pdfResult, "application/pdf", $"Result_{appointmentId}.pdf");
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetSlots([FromQuery] Guid serviceId, [FromQuery] DateTime date, [FromQuery] Guid? doctorId, [FromQuery] Guid? specializationId, CancellationToken ct)
        {
            var result = await timeSlotService.GetAvailableSlotsAsync(serviceId, date, doctorId, specializationId, ct);
            return Ok(result);
        }
    }
}
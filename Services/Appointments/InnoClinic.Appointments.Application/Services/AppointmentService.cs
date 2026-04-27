using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Services
{
    public class AppointmentService(IAppointmentRepository repository, ILogger<AppointmentService> logger, AppointmentMapper mapper, IValidator<AppointmentRequestDTO> validator) : IAppointmentService
    {
        public async Task<List<SpecializationResponseDTO>> GetSpecializationsAsync(string? officeId, CancellationToken ct)
        {
            logger.LogInformation("Fetching specializations for office: {OfficeId}", officeId);
            var data = await repository.GetSpecializationsAsync(officeId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<DoctorResponseDTO>> GetDoctorsAsync(Guid? specializationId, string? officeId, Guid? serviceId, CancellationToken ct)
        {
            logger.LogInformation("Fetching doctors. Filters - Spec: {SpecId}, Office: {OfficeId}, Service: {ServiceId}", specializationId, officeId, serviceId);
            var data = await repository.GetDoctorsAsync(specializationId, officeId, serviceId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<ServiceResponseDTO>> GetServicesAsync(Guid? specializationId, Guid? doctorId, string? officeId, CancellationToken ct)
        {
            logger.LogInformation("Fetching services. Filters - Spec: {SpecId}, Doctor: {DoctorId}", specializationId, doctorId);
            var data = await repository.GetServicesAsync(specializationId, doctorId, officeId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<OfficeResponseDTO>> GetOfficesAsync(Guid? specializationId, CancellationToken ct)
        {
            logger.LogInformation("Fetching offices for specialization: {SpecId}", specializationId);
            var data = await repository.GetOfficesAsync(specializationId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<AppointmentResponseDTO> CreateAppointmentAsync(AppointmentRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to create appointment for Patient: {PatientId} with Doctor: {DoctorId}", data.PatientId, data.DoctorId);

            var validationResult = await validator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                logger.LogWarning("Validation failed for appointment creation: {Errors}", errors);
                throw new BadRequestException(errors);
            }

            var isBusy = await repository.IsSlotBusyAsync(data.DoctorId, data.DateTime, ct);
            if (isBusy)
            {
                logger.LogWarning("Appointment failed. Slot busy: Doctor {DoctorId} at {DateTime}", data.DoctorId, data.DateTime);
                throw new BadRequestException("This time slot is already taken by another appointment.");
            }

            var appointment = mapper.ToEntity(data);

            repository.AddAppointment(appointment);
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Appointment successfully created with ID: {Id}", appointment.Id);

            var fullAppointment = await repository.GetAppointmentByIdAsync(appointment.Id, ct);

            return mapper.ToResponseDto(fullAppointment!);
        }

        public async Task<List<DoctorScheduleResponseDTO>> GetDoctorScheduleAsync(Guid doctorId, DateTime date, CancellationToken ct)
        {
            logger.LogInformation("Fetching schedule for Doctor: {DoctorId} on Date: {Date}", doctorId, date.ToShortDateString());

            var appointments = await repository.GetDoctorScheduleAsync(doctorId, date, ct);

            return appointments
                .Select(mapper.ToScheduleDto)
                .ToList();
        }
    }
}
using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Services
{
    public class AppointmentService(IAppointmentRepository repository, ILogger<AppointmentService> logger, AppointmentMapper mapper, IValidator<AppointmentRequestDTO> appointmentValidator, IValidator<ResultRequestDTO> resultValidator) : IAppointmentService
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

            var validationResult = await appointmentValidator.ValidateAsync(data, ct);
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

        public async Task<List<ReceptionistScheduleResponseDTO>> GetReceptionistAppointmentsAsync(DateTime? date, string? doctorName, string? serviceName, bool? isApproved, string? officeId, CancellationToken ct)
        {
            logger.LogInformation("Receptionist fetching appointments. Filters - Date: {Date}, Doctor: {Doctor}, Status: {Status}", date, doctorName, isApproved);

            var appointments = await repository.GetAppointmentsAsync(date, doctorName, serviceName, isApproved, officeId, ct);

            return appointments
                .Select(mapper.ToReceptionistDto)
                .ToList();
        }

        public async Task RescheduleAppointmentAsync(Guid id, DateTime newDateTime, CancellationToken ct)
        {
            logger.LogInformation("Rescheduling appointment {Id} to {NewDateTime}", id, newDateTime);

            var appointment = await repository.GetAppointmentByIdAsync(id, ct);
            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            var isBusy = await repository.IsSlotBusyAsync(appointment.DoctorId, newDateTime, ct);
            if (isBusy)
            {
                throw new BadRequestException("The new time slot is already taken.");
            }

            appointment.DateTime = DateTime.SpecifyKind(newDateTime, DateTimeKind.Utc);
            await repository.SaveChangesAsync(ct);
        }

        public async Task ApproveAppointmentAsync(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Approving appointment {Id}", id);

            var appointment = await repository.GetAppointmentByIdAsync(id, ct);
            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            appointment.IsConfirmed = true;
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Appointment {Id} successfully approved", id);
        }

        public async Task CancelAppointmentAsync(Guid id, CancellationToken ct)
        {
            logger.LogInformation("Canceling (deleting) appointment {Id}", id);

            var appointment = await repository.GetAppointmentByIdAsync(id, ct);
            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            repository.DeleteAppointment(appointment); 
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Appointment {Id} successfully canceled", id);
        }

        public async Task<List<PatientHistoryResponseDTO>> GetPatientHistoryAsync(Guid patientId, CancellationToken ct)
        {
            logger.LogInformation("Fetching history for patient: {PatientId}", patientId);
            var data = await repository.GetPatientHistoryAsync(patientId, ct);
            return data.Select(mapper.ToHistoryDto).ToList();
        }

        public async Task<ResultResponseDTO> CreateResultAsync(ResultRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to create medical result for appointment: {AppointmentId}", data.AppointmentId);

            var validationResult = await resultValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                logger.LogWarning("Validation failed for result: {Errors}", errors);
                throw new BadRequestException(errors);
            }

            var appointment = await repository.GetAppointmentByIdAsync(data.AppointmentId, ct);
            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            var exists = await repository.ResultExistsAsync(data.AppointmentId, ct);
            if (exists)
            {
                logger.LogWarning("Result already exists for appointment {Id}", data.AppointmentId);
                throw new BadRequestException("Medical result for this appointment has already been created.");
            }

            var resultEntity = mapper.ToEntity(data);

            repository.AddResult(resultEntity);
            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Medical result {Id} successfully created", resultEntity.Id);

            var fullResult = await repository.GetResultDetailsAsync(resultEntity.Id, ct);

            return mapper.ToResponseDto(fullResult!);
        }
    }
}
using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Application.Validators;
using InnoClinic.Appointments.Domain.Exceptions;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InnoClinic.Appointments.Application.Services
{
    public class AppointmentService(IAppointmentRepository repository, ILogger<AppointmentService> logger, AppointmentMapper mapper, IValidator<AppointmentRequestDTO> appointmentValidator, IValidator<ResultRequestDTO> resultValidator, IValidator<UpdateResultRequestDTO> updateResultValidator, IDocumentsClient documentsClient, IPublishEndpoint publishEndpoint) : IAppointmentService
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

            var patient = await repository.GetPatientByIdAsync(data.PatientId, ct) ?? throw new NotFoundException("Patient not found");
            var doctor = await repository.GetDoctorByIdAsync(data.DoctorId, ct) ?? throw new NotFoundException("Doctor not found");
            var service = await repository.GetServiceByIdAsync(data.ServiceId, ct) ?? throw new NotFoundException("Service not found");

            var appointment = mapper.ToEntity(data);
            appointment.Patient = patient;
            appointment.Doctor = doctor;
            appointment.Service = service;

            repository.AddAppointment(appointment);

            await publishEndpoint.Publish(new AppointmentCreatedEvent(
                appointment.Id,
                patient.FirstName,
                patient.LastName,
                patient.MiddleName,
                patient.Email,
                doctor.FirstName,
                doctor.LastName,
                doctor.MiddleName,
                service.Name,
                appointment.DateTime
            ), ct);

            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Appointment successfully created with ID: {Id}", appointment.Id);
            return mapper.ToResponseDto(appointment);
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

            Guid? documentId = null;

            if (data.File != null)
            {
                logger.LogInformation("File detected. Uploading to Documents API");

                documentId = await documentsClient.UploadDocumentAsync(data.File, ct);

                logger.LogInformation("File uploaded successfully. Received DocumentId: {DocumentId}", documentId);
            }

            var resultEntity = mapper.ToEntity(data);
            resultEntity.DocumentId = documentId;

            repository.AddResult(resultEntity);

            await publishEndpoint.Publish(new ResultCreatedEvent(
                appointment.Id,
                appointment.Patient.Email,
                $"{appointment.Patient.LastName} {appointment.Patient.FirstName}",
                $"{appointment.Doctor.LastName} {appointment.Doctor.FirstName}",
                appointment.Service.Name,
                appointment.DateTime,
                resultEntity.Complaints,
                resultEntity.Conclusion,
                resultEntity.Diagnosis,
                resultEntity.Recommendations
            ), ct);

            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Medical result {Id} successfully created", resultEntity.Id);

            var fullResult = await repository.GetResultAsync(r => r.Id == resultEntity.Id, ct);
            return mapper.ToResponseDto(fullResult!);
        }

        public async Task<ResultResponseDTO> UpdateResultAsync(Guid id, UpdateResultRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to update medical result {Id}", id);

            var validationResult = await updateResultValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new BadRequestException(errors);
            }

            var resultEntity = await repository.GetResultByIdAsync(id, ct);
            if (resultEntity == null)
            {
                throw new NotFoundException("Medical result not found");
            }

            mapper.UpdateResult(data, resultEntity);

            await publishEndpoint.Publish(new ResultUpdatedEvent(
                resultEntity.AppointmentId,
                resultEntity.Appointment.Patient.Email, 
                $"{resultEntity.Appointment.Patient.LastName} {resultEntity.Appointment.Patient.FirstName}",
                $"{resultEntity.Appointment.Doctor.LastName} {resultEntity.Appointment.Doctor.FirstName}",
                resultEntity.Appointment.Service.Name,
                resultEntity.Appointment.DateTime,
                resultEntity.Complaints,
                resultEntity.Conclusion,
                resultEntity.Diagnosis,
                resultEntity.Recommendations
            ), ct);

            await repository.SaveChangesAsync(ct);

            logger.LogInformation("Medical result {Id} successfully updated", id);

            var updatedFullResult = await repository.GetResultAsync(r => r.Id == resultEntity.Id, ct);
            return mapper.ToResponseDto(updatedFullResult!);
        }

        public async Task<ResultResponseDTO> GetResultByAppointmentIdAsync(Guid appointmentId, CancellationToken ct)
        {
            logger.LogInformation("Fetching medical result for appointment: {AppointmentId}", appointmentId);

            var result = await repository.GetResultAsync(r => r.AppointmentId == appointmentId, ct);

            if (result == null)
            {
                throw new NotFoundException("Medical result not found for this appointment.");
            }

            return mapper.ToResponseDto(result);
        }

        public async Task<byte[]> GenerateResultPdfAsync(Guid appointmentId, CancellationToken ct)
        {
            logger.LogInformation("Generating PDF result for appointment: {AppointmentId}", appointmentId);

            var result = await repository.GetResultAsync(r => r.AppointmentId == appointmentId, ct);
            if (result == null)
            {
                logger.LogWarning("PDF generation failed: Result for appointment {Id} not found", appointmentId);
                throw new NotFoundException("Medical result not found");
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Medical Report").FontSize(24).Bold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120);
                            columns.RelativeColumn();
                        });

                        AddRow(table, "Patient:", $"{result.Appointment.Patient.LastName} {result.Appointment.Patient.FirstName}");
                        AddRow(table, "Date of Birth:", result.Appointment.Patient.DateOfBirth.ToString());
                        AddRow(table, "Doctor:", $"{result.Appointment.Doctor.LastName} {result.Appointment.Doctor.FirstName}");
                        AddRow(table, "Service:", result.Appointment.Service.Name);
                        AddRow(table, "Diagnosis:", result.Diagnosis);
                        AddRow(table, "Complaints:", result.Complaints);
                        AddRow(table, "Conclusion:", result.Conclusion);
                        AddRow(table, "Recommendations:", result.Recommendations);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span(DateTime.Now.ToString("f"));
                    });
                });
            }).GeneratePdf();

            logger.LogInformation("PDF for appointment {Id} successfully generated", appointmentId);
            return pdfBytes;
        }

        private void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Text(label).Bold();
            table.Cell().Text(value);
        }
    }
}
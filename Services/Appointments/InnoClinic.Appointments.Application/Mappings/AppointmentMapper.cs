using Riok.Mapperly.Abstractions;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace InnoClinic.Appointments.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class AppointmentMapper(IConfiguration configuration)
    {
        private string GetTimeInterval(DateTime start)
        {
            var format = configuration["AppointmentSettings:TimeFormat"];
            var duration = configuration.GetValue<int>("AppointmentSettings:DefaultDurationMinutes");

            return $"{start.ToString(format)} - {start.AddMinutes(duration).ToString(format)}";
        }

        public partial SpecializationResponseDTO ToDto(Specialization specialization);
        public partial ServiceResponseDTO ToDto(Service service);
        public partial OfficeResponseDTO ToDto(Office office);
        public partial DoctorResponseDTO ToDto(Doctor doctor);

        public DoctorScheduleResponseDTO ToScheduleDto(Appointment entity)
        {
            return new DoctorScheduleResponseDTO(
                entity.Id,
                GetTimeInterval(entity.DateTime),
                entity.PatientId,
                $"{entity.Patient.LastName} {entity.Patient.FirstName} {entity.Patient.MiddleName}".Trim(),
                entity.Service.Name,
                entity.IsConfirmed
            );
        }

        public ReceptionistScheduleResponseDTO ToReceptionistDto(Appointment entity)
        {
            return new ReceptionistScheduleResponseDTO(
                entity.Id,
                GetTimeInterval(entity.DateTime),
                $"{entity.Doctor.LastName} {entity.Doctor.FirstName} {entity.Doctor.MiddleName}".Trim(),
                $"{entity.Patient.LastName} {entity.Patient.FirstName} {entity.Patient.MiddleName}".Trim(),
                entity.Patient.PhoneNumber,
                entity.Service.Name,
                entity.IsConfirmed
            );
        }

        public PatientHistoryResponseDTO ToHistoryDto(Appointment entity)
        {
            return new PatientHistoryResponseDTO(
                entity.Id,
                entity.DateTime.Date,
                GetTimeInterval(entity.DateTime),
                $"{entity.Doctor.LastName} {entity.Doctor.FirstName} {entity.Doctor.MiddleName}".Trim(),
                entity.Service.Name
            );
        }

        [MapperIgnoreTarget(nameof(Appointment.Id))]
        [MapperIgnoreTarget(nameof(Appointment.Doctor))]
        [MapperIgnoreTarget(nameof(Appointment.Service))]
        [MapperIgnoreTarget(nameof(Appointment.Patient))]
        [MapperIgnoreTarget(nameof(Appointment.IsConfirmed))]
        [MapperIgnoreTarget(nameof(Appointment.CreatedAt))]
        public partial Appointment ToEntity(AppointmentRequestDTO dto);

        public AppointmentResponseDTO ToResponseDto(Appointment entity)
        {
            return new AppointmentResponseDTO(
                entity.Id,
                entity.PatientId,
                entity.DoctorId,
                $"{entity.Doctor.LastName} {entity.Doctor.FirstName} {entity.Doctor.MiddleName}".Trim(),
                entity.ServiceId,
                entity.Service.Name,
                entity.DateTime,
                entity.IsConfirmed
            );
        }

        [MapperIgnoreTarget(nameof(Result.Id))]
        [MapperIgnoreTarget(nameof(Result.Appointment))]
        [MapperIgnoreTarget(nameof(Result.CreatedAt))]
        public partial Result ToEntity(ResultRequestDTO dto);

        public ResultResponseDTO ToResponseDto(Result entity)
        {
            var doctorSpecs = string.Join(", ", entity.Appointment.Doctor.Specializations.Select(s => s.Name));

            return new ResultResponseDTO(
                entity.Id,
                entity.CreatedAt,
                $"{entity.Appointment.Patient.LastName} {entity.Appointment.Patient.FirstName} {entity.Appointment.Patient.MiddleName}".Trim(),
                entity.Appointment.Patient.DateOfBirth,
                $"{entity.Appointment.Doctor.LastName} {entity.Appointment.Doctor.FirstName} {entity.Appointment.Doctor.MiddleName}".Trim(),
                doctorSpecs,
                entity.Appointment.Service.Name,
                entity.Complaints,
                entity.Conclusion,
                entity.Recommendations
            );
        }

        [MapperIgnoreTarget(nameof(Result.Id))]
        [MapperIgnoreTarget(nameof(Result.AppointmentId))]
        [MapperIgnoreTarget(nameof(Result.Appointment))]
        [MapperIgnoreTarget(nameof(Result.CreatedAt))]
        public partial void UpdateResult(UpdateResultRequestDTO dto, Result entity);
    }
}
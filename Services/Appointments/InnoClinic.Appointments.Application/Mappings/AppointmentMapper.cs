using Riok.Mapperly.Abstractions;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Domain.Models;

namespace InnoClinic.Appointments.Application.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
    public partial class AppointmentMapper
    {
        public partial SpecializationResponseDTO ToDto(Specialization specialization);
        public partial ServiceResponseDTO ToDto(Service service);
        public partial OfficeResponseDTO ToDto(Office office);
        public partial DoctorResponseDTO ToDto(Doctor doctor);

        public DoctorScheduleResponseDTO ToScheduleDto(Appointment entity)
        {
            var startTime = entity.DateTime.ToString("HH:mm");
            var endTime = entity.DateTime.AddMinutes(20).ToString("HH:mm");

            return new DoctorScheduleResponseDTO(
                entity.Id,
                $"{startTime} - {endTime}",
                entity.PatientId,
                $"{entity.Patient.LastName} {entity.Patient.FirstName} {entity.Patient.MiddleName}".Trim(),
                entity.Service.Name,
                entity.IsConfirmed
            );
        }

        public ReceptionistScheduleResponseDTO ToReceptionistDto(Appointment entity)
        {
            var startTime = entity.DateTime.ToString("HH:mm");
            var endTime = entity.DateTime.AddMinutes(20).ToString("HH:mm");

            return new ReceptionistScheduleResponseDTO(
                entity.Id,
                $"{startTime} - {endTime}",
                $"{entity.Doctor.LastName} {entity.Doctor.FirstName} {entity.Doctor.MiddleName}".Trim(),
                $"{entity.Patient.LastName} {entity.Patient.FirstName} {entity.Patient.MiddleName}".Trim(),
                entity.Patient.PhoneNumber,
                entity.Service.Name,
                entity.IsConfirmed
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
    }
}
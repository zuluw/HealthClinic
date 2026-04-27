namespace InnoClinic.Appointments.Application.DTOs
{
    public record AppointmentRequestDTO(Guid PatientId, Guid DoctorId, Guid ServiceId, Guid SpecializationId, string OfficeId, DateTime DateTime);
}

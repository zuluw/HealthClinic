namespace InnoClinic.Appointments.Application.DTOs
{
    public record AppointmentRequestDTO(Guid PatientId, Guid DoctorId, Guid ServiceId, DateTime DateTime);
}

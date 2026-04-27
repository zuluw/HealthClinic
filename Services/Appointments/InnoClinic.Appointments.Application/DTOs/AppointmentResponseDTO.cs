namespace InnoClinic.Appointments.Application.DTOs
{
    public record AppointmentResponseDTO(
        Guid Id,
        Guid PatientId,
        Guid DoctorId,
        string DoctorFullName,
        Guid ServiceId,
        string ServiceName,
        DateTime DateTime,
        bool IsConfirmed
    );
}
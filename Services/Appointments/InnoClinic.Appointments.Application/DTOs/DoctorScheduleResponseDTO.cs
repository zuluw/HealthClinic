namespace InnoClinic.Appointments.Application.DTOs
{
    public record DoctorScheduleResponseDTO(
        Guid AppointmentId,
        string TimeInterval,
        Guid PatientId,
        string PatientFullName,
        string ServiceName,
        bool IsApproved
    );
}
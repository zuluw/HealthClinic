namespace InnoClinic.Appointments.Application.DTOs
{
    public record ReceptionistScheduleResponseDTO(
        Guid Id,
        string TimeInterval,
        string DoctorFullName,
        string PatientFullName,
        string PatientPhoneNumber,
        string ServiceName,
        bool IsConfirmed
    );
}
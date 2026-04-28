namespace InnoClinic.Appointments.Application.DTOs
{
    public record PatientHistoryResponseDTO(
        Guid AppointmentId,
        DateTime Date,      
        string TimeInterval,
        string DoctorFullName,
        string ServiceName
    );
}
namespace InnoClinic.Appointments.Application.DTOs
{
    public record ResultRequestDTO(Guid AppointmentId, string Complaints, string Conclusion, string Recommendations);
}
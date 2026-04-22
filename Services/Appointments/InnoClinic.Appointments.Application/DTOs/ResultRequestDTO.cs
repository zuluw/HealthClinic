using Microsoft.AspNetCore.Http;

namespace InnoClinic.Appointments.Application.DTOs
{
    public record ResultRequestDTO(
        Guid AppointmentId, 
        string Complaints, 
        string Conclusion, 
        string Diagnosis, 
        string Recommendations,
        IFormFile? File
    );
}
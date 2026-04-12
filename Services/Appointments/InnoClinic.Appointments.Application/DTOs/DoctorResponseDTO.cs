namespace InnoClinic.Appointments.Application.DTOs
{
    public record DoctorResponseDTO(Guid Id, string FirstName, string LastName, string? MiddleName);
}

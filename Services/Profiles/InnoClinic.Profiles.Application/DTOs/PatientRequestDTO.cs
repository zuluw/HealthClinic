namespace InnoClinic.Profiles.Application.DTOs
{
    public record PatientRequestDTO(
        Guid AccountId,
        string FirstName,
        string LastName,
        string? MiddleName,
        DateOnly DateOfBirth,
        string PhoneNumber,
        string Email
    );
}
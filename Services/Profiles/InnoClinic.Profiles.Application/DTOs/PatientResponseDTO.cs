namespace InnoClinic.Profiles.Application.DTOs
{
    public record PatientResponseDTO(
        Guid Id,
        Guid AccountId,
        string FirstName,
        string LastName,
        string? MiddleName,
        DateOnly DateOfBirth,
        string PhoneNumber
    );
}
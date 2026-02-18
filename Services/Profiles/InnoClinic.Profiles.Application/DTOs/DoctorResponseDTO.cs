namespace InnoClinic.Profiles.Application.DTOs
{
    public record DoctorResponseDTO(
        Guid Id,
        Guid AccountId,
        string FirstName,
        string LastName,
        string? MiddleName,
        DateOnly DateOfBirth,
        DateOnly CareerStartDate,
        string Status,
        IEnumerable<string> SpecializationNames,
        Guid OfficeId
    );
}
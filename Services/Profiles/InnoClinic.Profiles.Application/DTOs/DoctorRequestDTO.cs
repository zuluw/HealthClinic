namespace InnoClinic.Profiles.Application.DTOs
{
    public record DoctorRequestDTO(
        Guid AccountId,
        string FirstName,
        string LastName,
        string? MiddleName,
        DateOnly DateOfBirth,
        DateOnly CareerStartDate,
        Guid OfficeId,
        IEnumerable<Guid> SpecializationIds
    );
}
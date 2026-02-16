namespace InnoClinic.Profiles.Domain.Models
{
    public class Patient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid AccountId { get; init; } 

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateOnly DateOfBirth { get; set; }

        public required string PhoneNumber { get; set; }
    }
}
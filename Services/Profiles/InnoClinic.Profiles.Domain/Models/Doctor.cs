namespace InnoClinic.Profiles.Domain.Models
{
    public enum DoctorStatus
    {
        AtWork = 1,
        OnLeave = 2,
        SickLeave = 3,
        Retired = 4
    }

    public class Doctor
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid AccountId { get; init; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateOnly DateOfBirth { get; set; }

        public Guid OfficeId { get; set; }
        public DateOnly CareerStartYear { get; set; }
        public DoctorStatus Status { get; set; } = DoctorStatus.AtWork;

        public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
    }
}
using InnoClinic.Appointments.Domain.Enums;

namespace InnoClinic.Appointments.Domain.Models
{
    public class Doctor
    {
        public Guid Id { get; init; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }

        public required string OfficeId { get; set; }
        public DoctorStatus Status { get; set; } = DoctorStatus.AtWork;

        public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
    }
}
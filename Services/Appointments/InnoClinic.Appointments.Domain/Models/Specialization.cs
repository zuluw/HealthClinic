namespace InnoClinic.Appointments.Domain.Models
{
    public class Specialization
    {
        public Guid Id { get; init; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
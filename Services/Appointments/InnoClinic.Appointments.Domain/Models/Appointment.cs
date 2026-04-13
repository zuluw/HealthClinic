namespace InnoClinic.Appointments.Domain.Models
{
    public class Appointment
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required Guid PatientId { get; set; }
        public virtual Patient Patient { get; set; } = null!;

        public required Guid DoctorId { get; set; }
        public virtual Doctor Doctor { get; set; } = null!;

        public required Guid ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;

        public required DateTime DateTime { get; set; }
        public bool IsConfirmed { get; set; } = false;

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}
namespace InnoClinic.Appointments.Domain.Models
{
    public class Result
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; } = null!;

        public required string Complaints { get; set; }
        public required string Conclusion { get; set; }
        public required string Diagnosis { get; set; }
        public required string Recommendations { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public Guid? DocumentId { get; set; }
    }
}
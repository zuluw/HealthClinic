using InnoClinic.Notifications.Domain.Enums;

namespace InnoClinic.Notifications.Domain.Models
{
    public class Notification
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required Guid AppointmentId { get; set; }

        public required string PatientFullName { get; set; }
        public required string PatientEmail { get; set; }
        public required string DoctorFullName { get; set; }
        public required string ServiceName { get; set; }
        public required DateTime AppointmentDateTime { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
    }
}
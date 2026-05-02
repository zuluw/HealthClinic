using InnoClinic.Notifications.Domain.Models;

namespace InnoClinic.Notifications.Application.Interfaces
{
    public interface INotificationRepository
    {
        void Add(Notification notification);
        Task<Notification?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct);
        Task<List<Notification>> GetPendingNotificationsAsync(DateTime from, DateTime to, CancellationToken ct); 
        void Update(Notification notification);
        Task SaveChangesAsync(CancellationToken ct);
    }
}
using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Domain.Enums;
using InnoClinic.Notifications.Domain.Models;
using InnoClinic.Notifications.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Notifications.Infrastructure.Repositories
{
    public class NotificationRepository(AppDbContext context) : INotificationRepository
    {
        public void Add(Notification notification)
        {
            context.Notifications.Add(notification);
        }

        public async Task<Notification?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct)
        {
            return await context.Notifications
                .FirstOrDefaultAsync(n => n.AppointmentId == appointmentId, ct);
        }

        public async Task<List<Notification>> GetPendingNotificationsAsync(DateTime from, DateTime to, CancellationToken ct)
        {
            return await context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending &&
                            n.AppointmentDateTime >= from &&
                            n.AppointmentDateTime < to)
                .ToListAsync(ct);
        }

        public void Update(Notification notification)
        {
            context.Notifications.Update(notification);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
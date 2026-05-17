using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Notifications.Application.Services
{
    public class NotificationService(INotificationRepository repository, IEmailService emailService, IConfiguration configuration, ILogger<NotificationService> logger) : INotificationService
    {
        public async Task SendDailyRemindersAsync(CancellationToken ct)
        {
            var daysStart = configuration.GetValue<int>("NotificationSettings:ReminderDaysStart");
            var windowDays = configuration.GetValue<int>("NotificationSettings:ReminderWindowDays");

            logger.LogInformation("Hangfire recurring job started: checking for upcoming appointments...");

            var startDate = DateTime.UtcNow.AddDays(daysStart).Date;
            var endDate = startDate.AddDays(windowDays);

            var notifications = await repository.GetPendingNotificationsAsync(startDate, endDate, ct);

            logger.LogInformation("Found {Count} pending notifications for the period {Start} to {End}", notifications.Count, startDate, endDate);

            foreach (var notification in notifications)
            {
                try
                {
                    await emailService.SendAppointmentAsync(
                        notification.PatientEmail,
                        notification.PatientFullName,
                        notification.AppointmentDateTime,
                        notification.ServiceName,
                        notification.DoctorFullName,
                        ct);

                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                    repository.Update(notification);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending notification for appointment {Id}", notification.AppointmentId);
                    notification.Status = NotificationStatus.Failed;
                }
            }

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Hangfire recurring job finished successfully.");
        }
    }
}
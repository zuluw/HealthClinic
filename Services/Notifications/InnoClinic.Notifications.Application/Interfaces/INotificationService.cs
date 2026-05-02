namespace InnoClinic.Notifications.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendDailyRemindersAsync(CancellationToken ct);
    }
}
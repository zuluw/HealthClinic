using InnoClinic.Common.Contracts.Events;

namespace InnoClinic.Notifications.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendDailyRemindersAsync(CancellationToken ct);
        Task ProcessAppointmentCreatedAsync(AppointmentCreatedEvent data, CancellationToken ct);
        Task ProcessResultCreatedAsync(ResultCreatedEvent data, CancellationToken ct);
        Task ProcessResultUpdatedAsync(ResultUpdatedEvent data, CancellationToken ct);
    }
}
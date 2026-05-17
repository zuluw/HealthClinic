using InnoClinic.Common.Contracts.Events;
using InnoClinic.Notifications.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Notifications.Application.Consumers
{
    public class AppointmentCreatedConsumer(INotificationService service, ILogger<AppointmentCreatedConsumer> logger) : IConsumer<AppointmentCreatedEvent>
    {
        public async Task Consume(ConsumeContext<AppointmentCreatedEvent> context)
        {
            await service.ProcessAppointmentCreatedAsync(context.Message, context.CancellationToken);
            logger.LogInformation("Sync: Notification record for appointment {Id} created", context.Message.Id);
        }
    }
}
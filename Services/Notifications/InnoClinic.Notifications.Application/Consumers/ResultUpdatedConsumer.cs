using InnoClinic.Common.Contracts.Events;
using InnoClinic.Notifications.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Notifications.Application.Consumers
{
    public class ResultUpdatedConsumer(INotificationService service, ILogger<ResultUpdatedConsumer> logger) : IConsumer<ResultUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<ResultUpdatedEvent> context)
        {
            await service.ProcessResultUpdatedAsync(context.Message, context.CancellationToken);
            logger.LogInformation("Process: Updated result for appointment {Id} processed and sent", context.Message.AppointmentId);
        }
    }
}
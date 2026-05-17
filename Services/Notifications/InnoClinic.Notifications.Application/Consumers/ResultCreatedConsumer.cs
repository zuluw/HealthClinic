using InnoClinic.Common.Contracts.Events;
using InnoClinic.Notifications.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Notifications.Application.Consumers
{
    public class ResultCreatedConsumer(INotificationService service, ILogger<ResultCreatedConsumer> logger) : IConsumer<ResultCreatedEvent>
    {
        public async Task Consume(ConsumeContext<ResultCreatedEvent> context)
        {
            await service.ProcessResultCreatedAsync(context.Message, context.CancellationToken);
            logger.LogInformation("Process: Result for appointment {Id} processed and sent", context.Message.AppointmentId);
        }
    }
}
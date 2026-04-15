using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Consumers
{
    public class ServiceStatusChangedConsumer(IAppointmentRepository repository, ILogger<ServiceStatusChangedConsumer> logger) : IConsumer<ServiceStatusChangedEvent>
    {
        public async Task Consume(ConsumeContext<ServiceStatusChangedEvent> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            var service = await repository.GetServiceByIdSyncAsync(msg.Id, ct);

            if (service != null)
            {
                service.IsActive = msg.IsActive;

                await repository.SaveChangesAsync(ct);
                logger.LogInformation("Sync: Service {Id} status changed to {Status}", msg.Id, msg.IsActive);
            }
        }
    }
}

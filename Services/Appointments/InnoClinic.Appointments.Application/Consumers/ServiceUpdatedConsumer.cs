using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Consumers
{
    public class ServiceUpdatedConsumer(IAppointmentRepository repository, ILogger<ServiceUpdatedConsumer> logger) : IConsumer<ServiceUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<ServiceUpdatedEvent> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            var service = await repository.GetServiceByIdSyncAsync(msg.Id, ct);

            if (service != null)
            {
                service.Name = msg.Name;
                service.Price = msg.Price;
                service.IsActive = msg.IsActive;

                await repository.SaveChangesAsync(ct);
                logger.LogInformation("Sync: Service {Id} updated", msg.Id);
            }
        }
    }
}

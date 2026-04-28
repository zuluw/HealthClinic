using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Consumers
{
    public class ServiceDeletedConsumer(IAppointmentRepository repository, ILogger<ServiceDeletedConsumer> logger) : IConsumer<ServiceDeletedEvent>
    {
        public async Task Consume(ConsumeContext<ServiceDeletedEvent> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            var service = await repository.GetServiceByIdSyncAsync(msg.Id, ct);

            if (service != null)
            {
                repository.DeleteService(service);

                await repository.SaveChangesAsync(ct);
                logger.LogInformation("Sync: Service {Id} deleted", msg.Id);
            }
        }
    }
}

using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Common.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Consumers
{
    public class ServiceCreatedConsumer(IAppointmentRepository repository, ILogger<ServiceCreatedConsumer> logger) : IConsumer<ServiceCreatedEvent>
    {
        public async Task Consume(ConsumeContext<ServiceCreatedEvent> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            var spec = await repository.GetSpecializationByIdAsync(msg.SpecializationId, ct);
            if (spec == null)
            {
                spec = new Specialization { Id = msg.SpecializationId, Name = "New Specialization", IsActive = true };
                repository.AddSpecialization(spec);
            }

            var service = new Service
            {
                Id = msg.Id,
                Name = msg.Name,
                Price = msg.Price,
                SpecializationId = msg.SpecializationId,
                IsActive = true
            };

            repository.AddService(service);

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Sync: Service {Id} created", msg.Id);
        }
    }
}
using Azure.Messaging.ServiceBus;
using InnoClinic.Notifications.Application.Consumers;
using MassTransit;
using Microsoft.Azure.Functions.Worker;

namespace InnoClinic.Notifications.Functions
{
    public class NotificationTriggers(IMessageReceiver receiver)
    {
        [Function("OnAppointmentCreated")]
        public async Task RunCreated([ServiceBusTrigger("appointment-created-event", "notifications-sub")] ServiceBusReceivedMessage message, CancellationToken ct)
        {
            await receiver.HandleConsumer<AppointmentCreatedConsumer>("appointment-created-event", message, ct);
        }

        [Function("OnResultCreated")]
        public async Task RunResult([ServiceBusTrigger("result-created-event", "notifications-sub")] ServiceBusReceivedMessage message, CancellationToken ct)
        {
            await receiver.HandleConsumer<ResultCreatedConsumer>("result-created-event", message, ct);
        }

        [Function("OnResultUpdated")]
        public async Task RunUpdated([ServiceBusTrigger("result-updated-event", "notifications-sub")] ServiceBusReceivedMessage message, CancellationToken ct)
        {
            await receiver.HandleConsumer<ResultUpdatedConsumer>("result-updated-event", message, ct);
        }
    }
}
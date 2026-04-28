using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Appointments.Application.Services
{
    public class TimeSlotService(IAppointmentRepository repository, ILogger<TimeSlotService> logger, IConfiguration configuration) : ITimeSlotService
    {
        private readonly TimeSpan _workDayStart = TimeSpan.Parse(configuration["AppointmentSettings:WorkDayStart"]);
        private readonly TimeSpan _workDayEnd = TimeSpan.Parse(configuration["AppointmentSettings:WorkDayEnd"]);
        private readonly int _slotMinutes = configuration.GetValue<int>("AppointmentSettings:SlotSizeMinutes");

        public async Task<List<AvailableSlotsResponseDTO>> GetAvailableSlotsAsync(Guid serviceId, DateTime date, Guid? doctorId, Guid? specializationId, CancellationToken ct)
        {
            logger.LogInformation("Generating available slots for Service: {ServiceId} on {Date}", serviceId, date.ToShortDateString());

            var service = await repository.GetServiceByIdAsync(serviceId, ct);
            if (service == null)
            {
                logger.LogWarning("Service {Id} not found during slot generation", serviceId);
                throw new NotFoundException("Service not found");
            }

            int slots = GetRequiredSlotsCount(service.Category);

            var existingAppointments = await repository.GetAppointmentsByDateAsync(date, doctorId, specializationId, ct);

            var timeline = GenerateTimeline();

            foreach (var app in existingAppointments)
            {
                var appStart = app.DateTime.TimeOfDay;
                var appSlotsCount = GetRequiredSlotsCount(app.Service.Category);

                for (int i = 0; i < appSlotsCount; i++)
                {
                    var busyTime = appStart.Add(TimeSpan.FromMinutes(i * _slotMinutes));
                    if (timeline.ContainsKey(busyTime)) 
                    {
                        timeline[busyTime] = false;
                    }
                }
            }

            var availableStarts = new List<AvailableSlotsResponseDTO>();
            var timeKeys = timeline.Keys.ToList();

            for (int i = 0; i <= timeKeys.Count - slots; i++)
            {
                bool isSequenceFree = true;
                for (int j = 0; j < slots; j++)
                {
                    if (!timeline[timeKeys[i + j]])
                    {
                        isSequenceFree = false;
                        break;
                    }
                }

                if (isSequenceFree)
                {
                    var slotTime = DateTime.SpecifyKind(date.Date.Add(timeKeys[i]), DateTimeKind.Utc);

                    if (slotTime > DateTime.UtcNow)
                    {
                        availableStarts.Add(new AvailableSlotsResponseDTO(slotTime, timeKeys[i].ToString(@"hh\:mm")));
                    }
                }
            }

            return availableStarts;
        }

        private int GetRequiredSlotsCount(ServiceCategory category) => category switch
        {
            ServiceCategory.Analyses => 1,
            ServiceCategory.Consultations => 2,
            ServiceCategory.Diagnostics => 3,
            _ => 1
        };

        private Dictionary<TimeSpan, bool> GenerateTimeline()
        {
            var timeline = new Dictionary<TimeSpan, bool>();
            var current = _workDayStart;
            while (current < _workDayEnd)
            {
                timeline.Add(current, true);
                current = current.Add(TimeSpan.FromMinutes(_slotMinutes));
            }
            return timeline;
        }
    }
}
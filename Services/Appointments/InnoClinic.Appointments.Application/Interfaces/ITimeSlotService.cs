using InnoClinic.Appointments.Application.DTOs;

namespace InnoClinic.Appointments.Application.Interfaces
{
    public interface ITimeSlotService
    {
        Task<List<AvailableSlotsResponseDTO>> GetAvailableSlotsAsync(Guid serviceId, DateTime date, Guid? doctorId, Guid? specializationId, CancellationToken ct);
    }
}

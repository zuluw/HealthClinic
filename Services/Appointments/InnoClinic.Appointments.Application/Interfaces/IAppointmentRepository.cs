using InnoClinic.Appointments.Domain.Models;

namespace InnoClinic.Appointments.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        void AddAppointment(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(Guid id, CancellationToken ct);
        Task<bool> IsSlotBusyAsync(Guid doctorId, DateTime dateTime, CancellationToken ct);

        Task<List<Specialization>> GetSpecializationsAsync(string? officeId, CancellationToken ct);
        Task<List<Doctor>> GetDoctorsAsync(Guid? specializationId, string? officeId, Guid? serviceId, CancellationToken ct);
        Task<List<Service>> GetServicesAsync(Guid? specializationId, Guid? doctorId, string? officeId, CancellationToken ct);
        Task<List<Office>> GetOfficesAsync(Guid? specializationId, CancellationToken ct);
        Task<List<Appointment>> GetDoctorScheduleAsync(Guid doctorId, DateTime date, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
using InnoClinic.Appointments.Domain.Models;
using System.Linq.Expressions;

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
        Task<List<Appointment>> GetAppointmentsAsync(DateTime? date, string? doctorName, string? serviceName, bool? isApproved, string? officeId, CancellationToken ct);
        void DeleteAppointment(Appointment appointment);
        Task<List<Appointment>> GetPatientHistoryAsync(Guid patientId, CancellationToken ct);
        void AddResult(Result result);
        Task<bool> ResultExistsAsync(Guid appointmentId, CancellationToken ct);
        Task<Result?> GetResultAsync(Expression<Func<Result, bool>> predicate, CancellationToken ct);
        Task<Result?> GetResultByIdAsync(Guid id, CancellationToken ct);
        Task<Service?> GetServiceByIdAsync(Guid id, CancellationToken ct);
        Task<List<Appointment>> GetAppointmentsByDateAsync(DateTime date, Guid? doctorId, Guid? specializationId, CancellationToken ct);

        void AddService(Service service);
        void DeleteService(Service service);
        Task<Service?> GetServiceByIdSyncAsync(Guid id, CancellationToken ct);
        void AddSpecialization(Specialization specialization);
        Task<Specialization?> GetSpecializationByIdAsync(Guid id, CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
    }
}
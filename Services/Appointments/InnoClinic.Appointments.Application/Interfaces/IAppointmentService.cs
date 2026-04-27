using InnoClinic.Appointments.Application.DTOs;

namespace InnoClinic.Appointments.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<SpecializationResponseDTO>> GetSpecializationsAsync(string? officeId, CancellationToken ct);
        Task<List<DoctorResponseDTO>> GetDoctorsAsync(Guid? specializationId, string? officeId, Guid? serviceId, CancellationToken ct);
        Task<List<ServiceResponseDTO>> GetServicesAsync(Guid? specializationId, Guid? doctorId, string? officeId, CancellationToken ct);
        Task<List<OfficeResponseDTO>> GetOfficesAsync(Guid? specializationId, CancellationToken ct);
        Task<AppointmentResponseDTO> CreateAppointmentAsync(AppointmentRequestDTO data, CancellationToken ct);
    }
}
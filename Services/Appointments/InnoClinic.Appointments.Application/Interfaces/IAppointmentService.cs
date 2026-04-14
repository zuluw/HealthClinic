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
        Task<List<DoctorScheduleResponseDTO>> GetDoctorScheduleAsync(Guid doctorId, DateTime date, CancellationToken ct);
        Task<List<ReceptionistScheduleResponseDTO>> GetReceptionistAppointmentsAsync(DateTime? date, string? doctorName, string? serviceName, bool? isApproved, string? officeId, CancellationToken ct);
        Task RescheduleAppointmentAsync(Guid id, DateTime newDateTime, CancellationToken ct);
        Task ApproveAppointmentAsync(Guid id, CancellationToken ct);
        Task CancelAppointmentAsync(Guid id, CancellationToken ct);
        Task<List<PatientHistoryResponseDTO>> GetPatientHistoryAsync(Guid patientId, CancellationToken ct);
        Task<ResultResponseDTO> CreateResultAsync(ResultRequestDTO data, CancellationToken ct);
        Task<ResultResponseDTO> UpdateResultAsync(Guid id, UpdateResultRequestDTO data, CancellationToken ct);
        Task<ResultResponseDTO> GetResultByAppointmentIdAsync(Guid appointmentId, CancellationToken ct);
        Task<byte[]> GenerateResultPdfAsync(Guid appointmentId, CancellationToken ct);
    }
}
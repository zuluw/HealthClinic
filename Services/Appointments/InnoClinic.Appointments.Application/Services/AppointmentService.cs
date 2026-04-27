using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;

namespace InnoClinic.Appointments.Application.Services
{
    public class AppointmentService(IAppointmentRepository repository, AppointmentMapper mapper) : IAppointmentService
    {
        public async Task<List<SpecializationResponseDTO>> GetSpecializationsAsync(string? officeId, CancellationToken ct)
        {
            var data = await repository.GetSpecializationsAsync(officeId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<DoctorResponseDTO>> GetDoctorsAsync(Guid? specializationId, string? officeId, Guid? serviceId, CancellationToken ct)
        {
            var data = await repository.GetDoctorsAsync(specializationId, officeId, serviceId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<ServiceResponseDTO>> GetServicesAsync(Guid? specializationId, Guid? doctorId, string? officeId, CancellationToken ct)
        {
            var data = await repository.GetServicesAsync(specializationId, doctorId, officeId, ct);
            return data.Select(mapper.ToDto).ToList();
        }

        public async Task<List<OfficeResponseDTO>> GetOfficesAsync(Guid? specializationId, CancellationToken ct)
        {
            var data = await repository.GetOfficesAsync(specializationId, ct);
            return data.Select(mapper.ToDto).ToList();
        }
    }
}
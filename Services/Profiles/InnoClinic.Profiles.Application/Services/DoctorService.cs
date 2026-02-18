using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Application.Mappings;
using InnoClinic.Profiles.Domain.Exceptions;
using InnoClinic.Profiles.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Profiles.Application.Services
{
    public class DoctorService(IDoctorRepository doctorRepository, IValidator<DoctorRequestDTO> doctorValidator, ILogger<DoctorService> logger, ProfileMapper mapper) : IDoctorService
    {
        public async Task<DoctorResponseDTO> CreateAsync(DoctorRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Creating doctor profile for AccountId: {AccountId}", data.AccountId);

            var validationResult = await doctorValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for doctor: {LastName}", data.LastName);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var profileResult = await doctorRepository.GetByAccountIdAsync(data.AccountId, ct);
            if (profileResult is not null)
            {
                logger.LogWarning("Profile creation failed: AccountId {AccountId} already has a profile", data.AccountId);
                throw new BadRequestException("Doctor profile already exists");
            }

            var doctor = mapper.ToEntity(data);

            var specs = await doctorRepository.GetSpecializationsByIdsAsync(data.SpecializationIds, ct);
            foreach (var s in specs)
            {
                doctor.Specializations.Add(s);
            }

            doctorRepository.Add(doctor);
            await doctorRepository.SaveChangesAsync(ct);

            logger.LogInformation("Doctor profile created successfully with ID: {Id}", doctor.Id);

            return mapper.ToResponse(doctor);
        }

        public async Task<DoctorResponseDTO> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found");
            }

            return mapper.ToResponse(doctor);
        }

        public async Task<DoctorResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByAccountIdAsync(accountId, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Profile not found");
            }

            return mapper.ToResponse(doctor);
        }

        public async Task<IEnumerable<DoctorResponseDTO>> GetBySpecializationAsync(Guid specializationId, CancellationToken ct)
        {
            var doctors = await doctorRepository.GetBySpecializationAsync(specializationId, ct);

            return doctors.Select(mapper.ToResponse);
        }

        public async Task<IEnumerable<DoctorResponseDTO>> GetAllAsync(CancellationToken ct)
        {
            var doctors = await doctorRepository.GetAllAsync(ct);

            return doctors.Select(mapper.ToResponse);
        }

        public async Task UpdateAsync(Guid id, DoctorRequestDTO data, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found");
            }

            mapper.UpdateDoctor(data, doctor);
            doctorRepository.Update(doctor);
            await doctorRepository.SaveChangesAsync(ct);
            
            logger.LogInformation("Doctor profile {Id} updated", id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found");
            }

            doctorRepository.Delete(doctor);
            await doctorRepository.SaveChangesAsync(ct);
            logger.LogInformation("Doctor profile {Id} deleted", id);
        }

        public async Task ChangeStatusAsync(Guid id, DoctorStatus newStatus, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct);

            if (doctor is null)
            {
                throw new NotFoundException("Doctor not found");
            }

            doctor.Status = newStatus;
            await doctorRepository.SaveChangesAsync(ct);
            logger.LogInformation("Doctor {Id} status changed to {Status}", id, newStatus);
        }
    }
}
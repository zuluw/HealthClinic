using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Application.Mappings;
using InnoClinic.Profiles.Domain.Exceptions;
using InnoClinic.Profiles.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Profiles.Application.Services
{
    public class PatientService(IPatientRepository patientRepository, IValidator<PatientRequestDTO> validator, ILogger<PatientService> logger, ProfileMapper mapper) : IPatientService
    {
        public async Task<PatientResponseDTO> CreateAsync(PatientRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Creating patient profile for Account: {AccountId}", data.AccountId);

            var validationResult = await validator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for patient: {LastName}", data.LastName);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            var profileResult = await patientRepository.GetByAccountIdAsync(data.AccountId, ct);
            if (profileResult is not null)
            {
                logger.LogWarning("Profile creation failed: AccountId {AccountId} already has a profile", data.AccountId);
                throw new BadRequestException("Patient profile already exists");
            }

            var patient = mapper.ToEntity(data);
            
            patientRepository.Add(patient);
            await patientRepository.SaveChangesAsync(ct);

            logger.LogInformation("Patient profile created successfully with ID: {Id}", patient.Id);

            return mapper.ToResponse(patient);
        }

        public async Task<PatientResponseDTO> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var patient = await patientRepository.GetByIdAsync(id, ct);

            if (patient is null)
            {
                throw new NotFoundException("Patient not found");
            }

            return mapper.ToResponse(patient);
        }

        public async Task<PatientResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        {
            var patient = await patientRepository.GetByAccountIdAsync(accountId, ct);

            if (patient is null)
            {
                throw new NotFoundException("Profile not found");
            }

            return mapper.ToResponse(patient);
        }

        public async Task<IEnumerable<PatientResponseDTO>> GetAllAsync(CancellationToken ct)
        {
            var patients = await patientRepository.GetAllAsync(ct);
            return patients.Select(mapper.ToResponse);
        }

        public async Task UpdateAsync(Guid id, PatientRequestDTO data, CancellationToken ct)
        {
            var patient = await patientRepository.GetByIdAsync(id, ct);

            if (patient is null)
            {
                logger.LogWarning("Update failed: Patient profile {Id} not found", id);
                throw new NotFoundException("Patient not found");
            }

            mapper.UpdatePatient(data, patient);

            patientRepository.Update(patient);
            await patientRepository.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var patient = await patientRepository.GetByIdAsync(id, ct);

            if (patient is null)
            {
                logger.LogWarning("Delete failed: Patient profile {Id} not found", id);
                throw new NotFoundException("Patient not found");
            }
            
            patientRepository.Delete(patient);
            await patientRepository.SaveChangesAsync(ct);
        }
    }
}
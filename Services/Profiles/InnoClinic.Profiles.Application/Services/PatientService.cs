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
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (await patientRepository.GetByAccountIdAsync(data.AccountId, ct) != null)
                throw new BadRequestException("Patient profile already exists");

            var patient = mapper.ToEntity(data);
            
            patientRepository.Add(patient);
            await patientRepository.SaveChangesAsync(ct);

            return mapper.ToResponse(patient);
        }

        public async Task<PatientResponseDTO> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var p = await patientRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Patient not found");
            return mapper.ToResponse(p);
        }

        public async Task<PatientResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        {
            var p = await patientRepository.GetByAccountIdAsync(accountId, ct) ?? throw new NotFoundException("Profile not found");
            return mapper.ToResponse(p);
        }

        public async Task<IEnumerable<PatientResponseDTO>> GetAllAsync(CancellationToken ct)
        {
            var patients = await patientRepository.GetAllAsync(ct);
            return patients.Select(mapper.ToResponse);
        }

        public async Task UpdateAsync(Guid id, PatientRequestDTO data, CancellationToken ct)
        {
            var p = await patientRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Patient not found");

            mapper.UpdatePatient(data, p);

            patientRepository.Update(p);
            await patientRepository.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var p = await patientRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Patient not found");
            patientRepository.Delete(p);
            await patientRepository.SaveChangesAsync(ct);
        }
    }
}
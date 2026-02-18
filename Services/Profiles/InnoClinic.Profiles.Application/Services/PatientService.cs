using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Exceptions;
using InnoClinic.Profiles.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Profiles.Application.Services
{
    public class PatientService(IPatientRepository patientRepository, IValidator<PatientRequestDTO> validator, ILogger<PatientService> logger) : IPatientService
    {
        public async Task<PatientResponseDTO> CreateAsync(PatientRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Creating patient profile for Account: {AccountId}", data.AccountId);

            var validationResult = await validator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            if (await patientRepository.GetByAccountIdAsync(data.AccountId, ct) != null)
                throw new BadRequestException("Patient profile already exists");

            var patient = new Patient
            {
                AccountId = data.AccountId,
                FirstName = data.FirstName,
                LastName = data.LastName,
                MiddleName = data.MiddleName,
                DateOfBirth = data.DateOfBirth,
                PhoneNumber = data.PhoneNumber
            };

            patientRepository.Add(patient);
            await patientRepository.SaveChangesAsync(ct);

            return new PatientResponseDTO(patient.Id, patient.AccountId, patient.FirstName,
                patient.LastName, patient.MiddleName, patient.DateOfBirth, patient.PhoneNumber);
        }

        public async Task<PatientResponseDTO> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var p = await patientRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Patient not found");
            return new PatientResponseDTO(p.Id, p.AccountId, p.FirstName, p.LastName, p.MiddleName, p.DateOfBirth, p.PhoneNumber);
        }

        public async Task<PatientResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        {
            var p = await patientRepository.GetByAccountIdAsync(accountId, ct) ?? throw new NotFoundException("Profile not found");
            return new PatientResponseDTO(p.Id, p.AccountId, p.FirstName, p.LastName, p.MiddleName, p.DateOfBirth, p.PhoneNumber);
        }

        public async Task<IEnumerable<PatientResponseDTO>> GetAllAsync(CancellationToken ct)
        {
            var patients = await patientRepository.GetAllAsync(ct);
            return patients.Select(p => new PatientResponseDTO(p.Id, p.AccountId, p.FirstName,
                p.LastName, p.MiddleName, p.DateOfBirth, p.PhoneNumber));
        }

        public async Task UpdateAsync(Guid id, PatientRequestDTO data, CancellationToken ct)
        {
            var p = await patientRepository.GetByIdAsync(id, ct) ?? throw new NotFoundException("Patient not found");

            p.FirstName = data.FirstName;
            p.LastName = data.LastName;
            p.MiddleName = data.MiddleName;
            p.PhoneNumber = data.PhoneNumber;

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
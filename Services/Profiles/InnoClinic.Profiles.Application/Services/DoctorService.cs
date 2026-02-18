using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Domain.Exceptions;
using InnoClinic.Profiles.Domain.Models;
using Microsoft.Extensions.Logging;

namespace InnoClinic.Profiles.Application.Services
{
    public class DoctorService(IDoctorRepository doctorRepository, IValidator<DoctorRequestDTO> doctorValidator, ILogger<DoctorService> logger) : IDoctorService
    {
        public async Task<DoctorResponseDTO> CreateAsync(DoctorRequestDTO data, CancellationToken ct)
        {
            logger.LogInformation("Attempting to create doctor profile for AccountId: {AccountId}", data.AccountId);

            var validationResult = await doctorValidator.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
            {
                logger.LogWarning("Validation failed for doctor: {LastName}", data.LastName);
                throw new BadRequestException(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            }

            if (await doctorRepository.GetByAccountIdAsync(data.AccountId, ct) != null)
            {
                logger.LogWarning("Profile creation failed: AccountId {AccountId} already has a profile", data.AccountId);
                throw new BadRequestException("Profile already exists");
            }

            var doctor = new Doctor
            {
                AccountId = data.AccountId,
                FirstName = data.FirstName,
                LastName = data.LastName,
                MiddleName = data.MiddleName,
                DateOfBirth = data.DateOfBirth,
                CareerStartDate = data.CareerStartDate,
                OfficeId = data.OfficeId,
                Status = DoctorStatus.AtWork
            };

            var specs = await doctorRepository.GetSpecializationsByIdsAsync(data.SpecializationIds, ct);
            foreach (var s in specs) doctor.Specializations.Add(s);

            doctorRepository.Add(doctor);
            await doctorRepository.SaveChangesAsync(ct);

            logger.LogInformation("Doctor profile created successfully with ID: {Id}", doctor.Id);

            return new DoctorResponseDTO(
                doctor.Id, doctor.AccountId, doctor.FirstName, doctor.LastName, doctor.MiddleName,
                doctor.DateOfBirth, doctor.CareerStartDate, doctor.Status.ToString(),
                doctor.Specializations.Select(s => s.Name), doctor.OfficeId
            );
        }

        public async Task<DoctorResponseDTO> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Doctor not found");

            return new DoctorResponseDTO(
                doctor.Id, doctor.AccountId, doctor.FirstName, doctor.LastName, doctor.MiddleName,
                doctor.DateOfBirth, doctor.CareerStartDate, doctor.Status.ToString(),
                doctor.Specializations.Select(s => s.Name), doctor.OfficeId
            );
        }

        public async Task<DoctorResponseDTO> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByAccountIdAsync(accountId, ct)
                ?? throw new NotFoundException("Profile not found");

            return new DoctorResponseDTO(
                doctor.Id, doctor.AccountId, doctor.FirstName, doctor.LastName, doctor.MiddleName,
                doctor.DateOfBirth, doctor.CareerStartDate, doctor.Status.ToString(),
                doctor.Specializations.Select(s => s.Name), doctor.OfficeId
            );
        }

        public async Task<IEnumerable<DoctorResponseDTO>> GetBySpecializationAsync(Guid specializationId, CancellationToken ct)
        {
            var doctors = await doctorRepository.GetBySpecializationAsync(specializationId, ct);

            return doctors.Select(d => new DoctorResponseDTO(
                d.Id, d.AccountId, d.FirstName, d.LastName, d.MiddleName,
                d.DateOfBirth, d.CareerStartDate, d.Status.ToString(),
                d.Specializations.Select(s => s.Name), d.OfficeId
            ));
        }

        public async Task<IEnumerable<DoctorResponseDTO>> GetAllAsync(CancellationToken ct)
        {
            var doctors = await doctorRepository.GetAllAsync(ct);

            return doctors.Select(d => new DoctorResponseDTO(
                d.Id, d.AccountId, d.FirstName, d.LastName, d.MiddleName,
                d.DateOfBirth, d.CareerStartDate, d.Status.ToString(),
                d.Specializations.Select(s => s.Name), d.OfficeId
            ));
        }

        public async Task UpdateAsync(Guid id, DoctorRequestDTO data, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Doctor not found");

            doctor.FirstName = data.FirstName;
            doctor.LastName = data.LastName;
            doctor.MiddleName = data.MiddleName;
            doctor.CareerStartDate = data.CareerStartDate;
            doctor.OfficeId = data.OfficeId;

            doctorRepository.Update(doctor);
            await doctorRepository.SaveChangesAsync(ct);
            logger.LogInformation("Doctor profile {Id} updated", id);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Doctor not found");

            doctorRepository.Delete(doctor);
            await doctorRepository.SaveChangesAsync(ct);
            logger.LogInformation("Doctor profile {Id} deleted", id);
        }

        public async Task ChangeStatusAsync(Guid id, DoctorStatus newStatus, CancellationToken ct)
        {
            var doctor = await doctorRepository.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Doctor not found");

            doctor.Status = newStatus;
            await doctorRepository.SaveChangesAsync(ct);
            logger.LogInformation("Doctor {Id} status changed to {Status}", id, newStatus);
        }
    }
}
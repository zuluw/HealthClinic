using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Appointments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Appointments.Infrastructure.Repositories
{
    public class AppointmentRepository(AppDbContext context) : IAppointmentRepository
    {
        public void AddAppointment(Appointment appointment) {
            context.Appointments.Add(appointment);
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(Guid id, CancellationToken ct)
        {
            return await context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Service) 
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<bool> IsSlotBusyAsync(Guid doctorId, DateTime dateTime, CancellationToken ct)
        {
            return await context.Appointments.AnyAsync(a => a.DoctorId == doctorId && a.DateTime == dateTime, ct);

        }

        public async Task<List<Specialization>> GetSpecializationsAsync(string? officeId, CancellationToken ct)
        {
            var query = context.Specializations.AsNoTracking();

            if (!string.IsNullOrEmpty(officeId))
            {
                query = query.Where(s => s.Doctors.Any(d => d.OfficeId == officeId));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<Doctor>> GetDoctorsAsync(Guid? specializationId, string? officeId, Guid? serviceId, CancellationToken ct)
        {
            IQueryable<Doctor> query = context.Doctors
                .AsNoTracking()
                .Include(d => d.Specializations);

            if (specializationId.HasValue)
            {
                query = query.Where(d => d.Specializations.Any(s => s.Id == specializationId));
            }

            if (!string.IsNullOrEmpty(officeId))
            {
                query = query.Where(d => d.OfficeId == officeId);
            }

            if (serviceId.HasValue)
            {
                query = query.Where(d => d.Specializations.Any(spec => spec.Services.Any(s => s.Id == serviceId)));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<Service>> GetServicesAsync(Guid? specializationId, Guid? doctorId, string? officeId, CancellationToken ct)
        {
            if (doctorId.HasValue)
            {
                var query = from doctor in context.Doctors
                            where doctor.Id == doctorId
                            from spec in doctor.Specializations
                            from service in spec.Services       
                            select service;

                return await query.Distinct().AsNoTracking().ToListAsync(ct);
            }

            if (specializationId.HasValue)
            {
                return await context.Services
                    .AsNoTracking()
                    .Where(s => s.SpecializationId == specializationId)
                    .ToListAsync(ct);
            }

            return await context.Services.AsNoTracking().ToListAsync(ct);
        }

        public async Task<List<Office>> GetOfficesAsync(Guid? specializationId, CancellationToken ct)
        {
            IQueryable<Office> query = context.Offices.AsNoTracking();

            if (specializationId.HasValue)
            {
                query = from office in context.Offices
                        join doctor in context.Doctors on office.Id equals doctor.OfficeId
                        where doctor.Specializations.Any(s => s.Id == specializationId)
                        select office;

                query = query.Distinct();
            }

            return await query.ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
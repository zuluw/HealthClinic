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
            var query = context.Services.AsNoTracking();

            if (specializationId.HasValue)
            {
                query = query.Where(s => s.SpecializationId == specializationId);
            }

            if (doctorId.HasValue)
            {
                query = query.Where(s => s.Specialization.Doctors.Any(d => d.Id == doctorId));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<Office>> GetOfficesAsync(Guid? specializationId, CancellationToken ct)
        {
            var query = context.Offices.AsNoTracking();

            if (specializationId.HasValue)
            {
                query = query.Where(o => context.Doctors.Any(d => d.OfficeId == o.Id && d.Specializations.Any(s => s.Id == specializationId)));
            }

            return await query.ToListAsync(ct);
        }

        public async Task<List<Appointment>> GetDoctorScheduleAsync(Guid doctorId, DateTime date, CancellationToken ct)
        {
            var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            return await context.Appointments
                .AsNoTracking() 
                .Include(a => a.Patient) 
                .Include(a => a.Service) 
                .Where(a => a.DoctorId == doctorId && a.DateTime >= startOfDay && a.DateTime < endOfDay)
                .OrderBy(a => a.DateTime)
                .ToListAsync(ct);
        }

        public async Task<List<Appointment>> GetAppointmentsAsync(DateTime? date, string? doctorName, string? serviceName, bool? isApproved, string? officeId, CancellationToken ct)
        {
            IQueryable<Appointment> query = context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Service);

            if (date.HasValue)
            {
                var startOfDay = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(a => a.DateTime >= startOfDay && a.DateTime < endOfDay);
            }

            if (!string.IsNullOrWhiteSpace(doctorName))
            {
                doctorName = doctorName.ToLower().Trim();
                query = query.Where(a =>(a.Doctor.LastName + " " + a.Doctor.FirstName + " " + (a.Doctor.MiddleName ?? "")).ToLower().Contains(doctorName));
            }

            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                serviceName = serviceName.ToLower();
                query = query.Where(a => a.Service.Name.ToLower().Contains(serviceName));
            }

            if (isApproved.HasValue)
            {
                query = query.Where(a => a.IsConfirmed == isApproved.Value);
            }

            if (!string.IsNullOrEmpty(officeId))
            {
                query = query.Where(a => a.Doctor.OfficeId == officeId);
            }

            return await query
                .OrderBy(a => a.DateTime)                     
                .ThenBy(a => a.Doctor.LastName)               
                .ThenBy(a => a.Doctor.FirstName)              
                .ThenBy(a => a.Service.Name)                  
                .ToListAsync(ct);
        }

        public void DeleteAppointment(Appointment appointment)
        {
            context.Appointments.Remove(appointment);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await context.SaveChangesAsync(ct);
        }
    }
}
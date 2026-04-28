using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Appointments.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Offices.AnyAsync())
            {
                return;
            }

            var officeId = "65f1a2b3c4d5e6f7a8b9c0d1";
            var specId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doctorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var serviceId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var patientId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var appointmentId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var staticDate = new DateTime(2026, 10, 10, 14, 0, 0, DateTimeKind.Utc);

            var office = new Office 
            { 
                Id = officeId, 
                Address = "123 Main St, New York", 
                IsActive = true 
            };

            var specialization = new Specialization 
            { 
                Id = specId, 
                Name = "Cardiology", 
                IsActive = true 
            };

            var service = new Service
            {
                Id = serviceId,
                Name = "General Consultation",
                Price = 100.00m,
                IsActive = true,
                SpecializationId = specId,
            };

            var patient = new Patient
            {
                Id = patientId,
                AccountId = Guid.NewGuid(),
                FirstName = "Sasha",
                LastName = "Petrov",
                MiddleName = "Alexandrovich",
                PhoneNumber = "+375291234567",
                DateOfBirth = new DateOnly(1995, 5, 20)
            };

            var doctor = new Doctor
            {
                Id = doctorId,
                FirstName = "Ivan",
                LastName = "Ivanov",
                MiddleName = "Ivanovich",
                OfficeId = officeId,
                Status = DoctorStatus.AtWork
            };

            doctor.Specializations.Add(specialization);

            var appointment = new Appointment
            {
                Id = appointmentId,
                PatientId = patientId,
                DoctorId = doctorId,
                ServiceId = serviceId,
                DateTime = staticDate,
                IsConfirmed = true,
                CreatedAt = staticDate
            };

            context.Offices.Add(office);
            context.Specializations.Add(specialization);
            context.Services.Add(service);
            context.Patients.Add(patient);
            context.Doctors.Add(doctor);
            context.Appointments.Add(appointment);

            await context.SaveChangesAsync();
        }
    }
}
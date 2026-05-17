using FluentAssertions;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Domain.Enums;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Appointments.IntegrationTests.Shared;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InnoClinic.Appointments.IntegrationTests.Controllers
{
    public class AppointmentsControllerTests : IntegrationTestBase
    {
        public AppointmentsControllerTests(ApiWebApplicationFactory factory) : base(factory) { }

        private async Task<Specialization> SeedSpecialization(string? name = null)
        {
            var specName = name ?? $"Spec_{Guid.NewGuid().ToString()[..8]}";
            var spec = new Specialization 
            { 
                Id = Guid.NewGuid(), 
                Name = specName, 
                IsActive = true 
            };
            
            DbContext.Specializations.Add(spec);
            await DbContext.SaveChangesAsync();
            
            return spec;
        }

        private async Task<Office> SeedOffice(string? id = null, string address = "Main St 1")
        {
            var officeId = id ?? Guid.NewGuid().ToString()[..20];
            var office = new Office 
            { 
                Id = officeId, 
                Address = address, 
                IsActive = true 
            };
            
            DbContext.Offices.Add(office);
            await DbContext.SaveChangesAsync();
            
            return office;
        }

        private async Task<Doctor> SeedDoctor(string firstName, string lastName, string officeId, Specialization spec, Guid? id = null)
        {
            var doctor = new Doctor
            {
                Id = id ?? Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                OfficeId = officeId,
                Status = DoctorStatus.AtWork
            };
            
            doctor.Specializations.Add(spec);

            DbContext.Doctors.Add(doctor);
            await DbContext.SaveChangesAsync();
            
            return doctor;
        }

        private async Task<Service> SeedService(string name, Specialization spec, ServiceCategory cat = ServiceCategory.Consultations)
        {
            var service = new Service
            {
                Id = Guid.NewGuid(),
                Name = name,
                Price = 100,
                SpecializationId = spec.Id,
                IsActive = true,
                Category = cat
            };
            
            DbContext.Services.Add(service);
            await DbContext.SaveChangesAsync();
            
            return service;
        }

        private async Task<Patient> SeedPatient(string? email = null)
        {
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                AccountId = Guid.NewGuid(),
                FirstName = "Sasha",
                LastName = "Ivanov",
                Email = email ?? $"{Guid.NewGuid()}@test.com",
                PhoneNumber = "123",
                DateOfBirth = new DateOnly(1990, 1, 1)
            };
            
            DbContext.Patients.Add(patient);
            await DbContext.SaveChangesAsync();
            
            return patient;
        }

        private async Task<Appointment> SeedAppointment(Guid? doctorId = null, DateTime? dateTime = null, bool isConfirmed = false)
        {
            Guid finalDocId;

            if (doctorId.HasValue)
            {
                var existingDoctor = await DbContext.Doctors.AnyAsync(d => d.Id == doctorId.Value);
                if (!existingDoctor)
                {
                    var spec = await SeedSpecialization();
                    var office = await SeedOffice();
                    await SeedDoctor("Test", "Doctor", office.Id, spec, doctorId.Value);
                }
                finalDocId = doctorId.Value;
            }
            else
            {
                var spec = await SeedSpecialization();
                var office = await SeedOffice();
                var doc = await SeedDoctor("Gregory", "House", office.Id, spec);
                finalDocId = doc.Id;
            }

            var patient = await SeedPatient($"{Guid.NewGuid()}@test.com");
            var specForService = await SeedSpecialization();
            var service = await SeedService("General", specForService);

            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = patient.Id,
                DoctorId = finalDocId,
                ServiceId = service.Id,
                DateTime = DateTime.SpecifyKind(dateTime ?? DateTime.UtcNow.AddDays(1), DateTimeKind.Utc),
                IsConfirmed = isConfirmed
            };

            DbContext.Appointments.Add(appointment);
            await DbContext.SaveChangesAsync();
            
            return appointment;
        }

        private async Task<Result> SeedResult(Guid appointmentId)
        {
            var result = new Result
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                Complaints = "Persistent headache",
                Diagnosis = "Migraine",
                Conclusion = "Patient needs rest",
                Recommendations = "Avoid bright light"
            };
            
            DbContext.Results.Add(result);
            await DbContext.SaveChangesAsync();
            
            return result;
        }

        [Fact]
        public async Task GetSpecializations_ShouldReturnActiveSpecializations()
        {
            // Arrange
            await SeedSpecialization("Surgery");

            // Act
            var response = await Client.GetAsync("/api/appointments/specializations");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<SpecializationResponseDTO>>();
            result.Should().Contain(x => x.Name == "Surgery");
        }

        [Fact]
        public async Task GetOffices_WhenFilteredBySpecialization_ShouldReturnRelevantOffices()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office1 = await SeedOffice("off-1");
            await SeedDoctor("Doc", "1", office1.Id, spec);

            // Act
            var response = await Client.GetAsync($"/api/appointments/offices?specializationId={spec.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<OfficeResponseDTO>>();
            result.Should().HaveCount(1);
            result!.First().Id.Should().Be(office1.Id);
        }

        [Fact]
        public async Task CreateAppointment_WhenRequestIsValid_ShouldSaveInDb()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doctor = await SeedDoctor("Gregory", "House", office.Id, spec);
            var patient = await SeedPatient();
            var service = await SeedService("Consultation", spec, ServiceCategory.Consultations);

            var request = new AppointmentRequestDTO(
                patient.Id, doctor.Id, service.Id, spec.Id, office.Id,
                DateTime.UtcNow.AddDays(1).Date.AddHours(10)
            );

            // Act
            var response = await Client.PostAsJsonAsync("/api/appointments", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var dbApp = await DbContext.Appointments.AnyAsync(a => a.PatientId == patient.Id);
            dbApp.Should().BeTrue();
        }

        [Fact]
        public async Task GetSlots_WhenDoctorIsBusy_ShouldExcludeBusyTimes()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doctor = await SeedDoctor("Eric", "Foreman", office.Id, spec);
            var service = await SeedService("Consultation", spec, ServiceCategory.Consultations);

            var targetDate = DateTime.UtcNow.AddDays(5).Date;
            var busyTime = DateTime.SpecifyKind(targetDate.AddHours(9), DateTimeKind.Utc);
            await SeedAppointment(doctorId: doctor.Id, dateTime: busyTime);

            // Act
            var response = await Client.GetAsync($"/api/appointments/available-slots?serviceId={service.Id}&date={targetDate:yyyy-MM-dd}&doctorId={doctor.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var slots = await response.Content.ReadFromJsonAsync<List<AvailableSlotsResponseDTO>>();
            slots.Should().NotContain(x => x.TimeLabel == "09:00");
        }

        [Fact]
        public async Task ApproveAppointment_WhenAppointmentExists_ShouldUpdateStatus()
        {
            // Arrange
            var app = await SeedAppointment(isConfirmed: false);

            // Act
            var response = await Client.PatchAsync($"/api/appointments/{app.Id}/approve", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var updated = await DbContext.Appointments.AsNoTracking().FirstAsync(a => a.Id == app.Id);
            updated.IsConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task CancelAppointment_WhenAppointmentExists_ShouldRemoveFromDb()
        {
            // Arrange
            var app = await SeedAppointment();

            // Act
            var response = await Client.DeleteAsync($"/api/appointments/{app.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var dbApp = await DbContext.Appointments.AnyAsync(a => a.Id == app.Id);
            dbApp.Should().BeFalse();
        }

        [Fact]
        public async Task GetMySchedule_WhenAppointmentExists_ShouldReturnDoctorSchedule()
        {
            // Arrange
            var myId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var testDate = DateTime.UtcNow.AddDays(1).Date;
            var appointment = await SeedAppointment(doctorId: myId, dateTime: testDate.AddHours(10));

            // Act
            var response = await Client.GetAsync($"/api/appointments/my-schedule?date={testDate:yyyy-MM-dd}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<DoctorScheduleResponseDTO>>();
            result.Should().NotBeEmpty();
            result.Should().Contain(x => x.AppointmentId == appointment.Id);
        }

        [Fact]
        public async Task GetPatientHistory_WhenHistoryExists_ShouldReturnCorrectData()
        {
            // Arrange
            var app = await SeedAppointment();

            // Act
            var response = await Client.GetAsync($"/api/appointments/patient/{app.PatientId}/history");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<PatientHistoryResponseDTO>>();
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateResult_WhenRequestIsValid_ShouldSaveResult()
        {
            // Arrange
            var app = await SeedAppointment();

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(app.Id.ToString()), "AppointmentId");
            content.Add(new StringContent("Complaints text"), "Complaints");
            content.Add(new StringContent("Conclusion text"), "Conclusion");
            content.Add(new StringContent("Flu"), "Diagnosis");
            content.Add(new StringContent("Rest"), "Recommendations");

            // Act
            var response = await Client.PostAsync("/api/appointments/result", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await DbContext.Results.FirstOrDefaultAsync(r => r.AppointmentId == app.Id);
            result.Should().NotBeNull();
            result!.Diagnosis.Should().Be("Flu");
        }

        [Fact]
        public async Task UpdateResult_WhenRequestIsValid_ShouldModifyData()
        {
            // Arrange
            var app = await SeedAppointment();
            var result = await SeedResult(app.Id);
            var updateRequest = new UpdateResultRequestDTO("New Complaints", "New Conclusion", "New Diagnosis", "New Recs");

            // Act
            var response = await Client.PutAsJsonAsync($"/api/appointments/results/{result.Id}", updateRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var updatedInDb = await DbContext.Results.AsNoTracking().FirstAsync(r => r.Id == result.Id);
            updatedInDb.Diagnosis.Should().Be("New Diagnosis");
        }

        [Fact]
        public async Task DownloadResultPdf_WhenResultExists_ShouldReturnPdfStream()
        {
            // Arrange
            var app = await SeedAppointment();
            await SeedResult(app.Id);

            // Act
            var response = await Client.GetAsync($"/api/appointments/{app.Id}/result/pdf");

            // Assert
            response.EnsureSuccessStatusCode();
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
            var stream = await response.Content.ReadAsByteArrayAsync();
            stream.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Reschedule_WhenTargetSlotIsBusy_ShouldReturnBadRequest()
        {
            // Arrange
            var doctor = await SeedDoctor("House", "MD", (await SeedOffice()).Id, await SeedSpecialization());
            var app1 = await SeedAppointment(doctorId: doctor.Id, dateTime: DateTime.UtcNow.AddDays(2).Date.AddHours(10));
            var app2 = await SeedAppointment(doctorId: doctor.Id, dateTime: DateTime.UtcNow.AddDays(2).Date.AddHours(11));

            // Act
            var response = await Client.PatchAsync($"/api/appointments/{app2.Id}/reschedule?newDateTime={app1.DateTime:O}", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReceptionistAppointments_WhenFilteredByName_ShouldReturnMatched()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doc = await SeedDoctor("Gregory", "House", office.Id, spec);
            await SeedAppointment(doctorId: doc.Id);

            // Act
            var response = await Client.GetAsync($"/api/appointments/receptionist?doctorName=House");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<ReceptionistScheduleResponseDTO>>();
            result.Should().Contain(x => x.DoctorFullName.Contains("House"));
        }

        [Fact]
        public async Task CreateAppointment_WhenDateInPast_ShouldReturnBadRequest()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doctor = await SeedDoctor("Doc", "Test", office.Id, spec);
            var patient = await SeedPatient();
            var service = await SeedService("Check", spec);

            var pastDate = DateTime.UtcNow.AddDays(-1);
            var request = new AppointmentRequestDTO(patient.Id, doctor.Id, service.Id, spec.Id, office.Id, pastDate);

            // Act
            var response = await Client.PostAsJsonAsync("/api/appointments", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDoctors_WhenAllFilters_ShouldReturnCorrectDoctor()
        {
            // Arrange
            var spec = await SeedSpecialization("Neurology");
            var office = await SeedOffice("off-unique-1");
            var service = await SeedService("Nerve Test", spec);
            var doc = await SeedDoctor("Gregory", "House", office.Id, spec);

            // Act
            var response = await Client.GetAsync($"/api/appointments/doctors?specializationId={spec.Id}&officeId={office.Id}&serviceId={service.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<DoctorResponseDTO>>();
            result.Should().Contain(x => x.Id == doc.Id);
        }

        [Fact]
        public async Task GetServices_WhenFilteredByDoctor_ShouldReturnHisServices()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doc = await SeedDoctor("James", "Wilson", office.Id, spec);
            var service = await SeedService("Oncology Consultation", spec);

            // Act
            var response = await Client.GetAsync($"/api/appointments/services?doctorId={doc.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<ServiceResponseDTO>>();
            result.Should().Contain(x => x.Id == service.Id);
        }

        [Fact]
        public async Task Reschedule_WhenRequestIsValid_ShouldChangeDateTime()
        {
            // Arrange
            var app = await SeedAppointment();
            var newTime = DateTime.UtcNow.AddDays(10).Date.AddHours(14);
            newTime = DateTime.SpecifyKind(newTime, DateTimeKind.Utc);

            // Act
            var response = await Client.PatchAsync($"/api/appointments/{app.Id}/reschedule?newDateTime={newTime:O}", null);

            // Assert
            response.EnsureSuccessStatusCode();
            var updated = await DbContext.Appointments.AsNoTracking().FirstAsync(a => a.Id == app.Id);
            updated.DateTime.Should().BeCloseTo(newTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task GetDoctorSchedule_WhenAppointmentExists_ShouldReturnDoctorSchedule()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doc = await SeedDoctor("Eric", "Foreman", office.Id, spec);
            var date = DateTime.UtcNow.AddDays(3).Date;
            await SeedAppointment(doctorId: doc.Id, dateTime: date.AddHours(10));

            // Act
            var response = await Client.GetAsync($"/api/appointments/doctor/{doc.Id}/schedule?date={date:yyyy-MM-dd}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<DoctorScheduleResponseDTO>>();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task ApproveAppointment_WhenAppointmentDoesNotExist_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.PatchAsync($"/api/appointments/{Guid.NewGuid()}/approve", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateResult_WhenDuplicate_ShouldReturnBadRequest()
        {
            // Arrange
            var app = await SeedAppointment();
            await SeedResult(app.Id);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(app.Id.ToString()), "AppointmentId");
            content.Add(new StringContent("Some complaints"), "Complaints");
            content.Add(new StringContent("Some diagnosis"), "Diagnosis");
            content.Add(new StringContent("Some conclusion"), "Conclusion");
            content.Add(new StringContent("Recs"), "Recommendations");

            // Act
            var response = await Client.PostAsync("/api/appointments/result", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetResult_WhenResultDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var app = await SeedAppointment();

            // Act
            var response = await Client.GetAsync($"/api/appointments/{app.Id}/result");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReceptionistAppointments_WhenFilteredByService_ShouldReturnMatched()
        {
            // Arrange
            var spec = await SeedSpecialization();
            var office = await SeedOffice();
            var doc = await SeedDoctor("Robert", "Chase", office.Id, spec);
            var service1 = await SeedService("Blood Test", spec);
            var service2 = await SeedService("X-Ray", spec);
            var patient = await SeedPatient();

            DbContext.Appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doc.Id,
                ServiceId = service1.Id,
                DateTime = DateTime.UtcNow.AddDays(1),
                IsConfirmed = true
            });
            DbContext.Appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                DoctorId = doc.Id,
                ServiceId = service2.Id,
                DateTime = DateTime.UtcNow.AddDays(2),
                IsConfirmed = true
            });
            await DbContext.SaveChangesAsync();

            // Act
            var response = await Client.GetAsync("/api/appointments/receptionist?serviceName=X-Ray");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<ReceptionistScheduleResponseDTO>>();
            result.Should().OnlyContain(x => x.ServiceName.Contains("X-Ray"));
            result.Should().HaveCount(1);
        }
    }
}
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Appointments.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace InnoClinic.Appointments.UnitTests.Mappings
{
    public class AppointmentMapperTests
    {
        private readonly AppointmentMapper _mapper;

        public AppointmentMapperTests()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["AppointmentSettings:TimeFormat"]).Returns("HH:mm");
            configMock.Setup(c => c.GetSection("AppointmentSettings:DefaultDurationMinutes").Value).Returns("30");

            _mapper = new AppointmentMapper(configMock.Object);
        }

        [Fact]
        public void GetTimeInterval_ShouldCalculateCorrectEndAndFormat()
        {
            // Arrange
            var appointment = new Appointment
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                DateTime = new DateTime(2025, 5, 20, 14, 30, 0, DateTimeKind.Utc),
                Doctor = new Doctor { FirstName = "A", LastName = "B", OfficeId = "1" },
                Patient = new Patient { FirstName = "C", LastName = "D", Email = "e", PhoneNumber = "p", AccountId = Guid.NewGuid() },
                Service = new Service { Name = "S", SpecializationId = Guid.NewGuid() }
            };

            // Act
            var result = _mapper.ToScheduleDto(appointment);

            // Assert
            result.TimeInterval.Should().Be("14:30 - 15:00");
        }

        [Fact]
        public void ToScheduleDto_WhenMiddleNameIsNull_ShouldNotHaveDoubleSpaces()
        {
            // Arrange
            var appointment = new Appointment
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                Patient = new Patient
                {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    MiddleName = null,
                    Email = "e",
                    PhoneNumber = "p",
                    AccountId = Guid.NewGuid()
                },
                Doctor = new Doctor { FirstName = "A", LastName = "B", OfficeId = "1" },
                Service = new Service { Name = "S", SpecializationId = Guid.NewGuid() }
            };

            // Act
            var result = _mapper.ToScheduleDto(appointment);

            // Assert
            result.PatientFullName.Should().Be("Ivanov Ivan");
            result.PatientFullName.Should().NotContain("  ");
        }

        [Fact]
        public void ToResponseDto_ForResult_ShouldJoinMultipleSpecializations()
        {
            // Arrange
            var resultEntity = new Result
            {
                Complaints = "C",
                Conclusion = "C",
                Diagnosis = "D",
                Recommendations = "R",
                CreatedAt = DateTime.UtcNow,
                Appointment = new Appointment
                {
                    PatientId = Guid.NewGuid(),
                    DoctorId = Guid.NewGuid(),
                    ServiceId = Guid.NewGuid(),
                    DateTime = DateTime.UtcNow,
                    Doctor = new Doctor
                    {
                        FirstName = "House",
                        LastName = "Gregory",
                        OfficeId = "1",
                        Specializations = new List<Specialization>
                        {
                            new() { Name = "Cardiology" },
                            new() { Name = "Surgery" }
                        }
                    },
                    Patient = new Patient { FirstName = "A", LastName = "B", Email = "e", PhoneNumber = "p", AccountId = Guid.NewGuid() },
                    Service = new Service { Name = "S", SpecializationId = Guid.NewGuid() }
                }
            };

            // Act
            var resultDto = _mapper.ToResponseDto(resultEntity);

            // Assert
            resultDto.DoctorSpecialization.Should().Be("Cardiology, Surgery");
        }

        [Fact]
        public void UpdateResult_ShouldModifyExistingEntity()
        {
            // Arrange
            var entity = new Result
            {
                Complaints = "Old",
                Conclusion = "Old",
                Diagnosis = "Old",
                Recommendations = "Old",
                AppointmentId = Guid.NewGuid()
            };
            var dto = new UpdateResultRequestDTO("NewComplaints", "NewConclusion", "NewDiagnosis", "NewRecs");

            // Act
            _mapper.UpdateResult(dto, entity);

            // Assert
            entity.Complaints.Should().Be("NewComplaints");
            entity.Diagnosis.Should().Be("NewDiagnosis");
            entity.Conclusion.Should().Be("NewConclusion");
        }

        [Fact]
        public void ToReceptionistDto_ShouldFormatAllNamesAndInterval()
        {
            // Arrange
            var entity = new Appointment
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                DateTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                Doctor = new Doctor { FirstName = "Gregory", LastName = "House", MiddleName = "R.", OfficeId = "1" },
                Patient = new Patient { FirstName = "John", LastName = "Doe", MiddleName = null, Email = "e", PhoneNumber = "12345", AccountId = Guid.NewGuid() },
                Service = new Service { Name = "X-Ray", SpecializationId = Guid.NewGuid() }
            };

            // Act
            var dto = _mapper.ToReceptionistDto(entity);

            // Assert
            dto.DoctorFullName.Should().Be("House Gregory R.");
            dto.PatientFullName.Should().Be("Doe John");
            dto.TimeInterval.Should().Be("12:00 - 12:30");
            dto.PatientPhoneNumber.Should().Be("12345");
        }

        [Fact]
        public void ToHistoryDto_ShouldFormatDateAndInterval()
        {
            // Arrange
            var date = new DateTime(2025, 12, 31, 23, 30, 0, DateTimeKind.Utc);
            var entity = new Appointment
            {
                PatientId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                DateTime = date,
                Doctor = new Doctor { FirstName = "A", LastName = "B", OfficeId = "1" },
                Patient = new Patient { FirstName = "C", LastName = "D", Email = "e", PhoneNumber = "p", AccountId = Guid.NewGuid() },
                Service = new Service { Name = "Surgery", SpecializationId = Guid.NewGuid() }
            };

            // Act
            var dto = _mapper.ToHistoryDto(entity);

            // Assert
            dto.Date.Should().Be(date.Date);
            dto.ServiceName.Should().Be("Surgery");
        }

        [Fact]
        public void ToEntity_FromAppointmentRequest_ShouldMapCorrectly()
        {
            // Arrange
            var dto = new AppointmentRequestDTO(
                PatientId: Guid.NewGuid(),
                DoctorId: Guid.NewGuid(),
                ServiceId: Guid.NewGuid(),
                SpecializationId: Guid.NewGuid(),
                OfficeId: "Office-777",
                DateTime: DateTime.UtcNow
            );

            // Act
            var entity = _mapper.ToEntity(dto);

            // Assert
            entity.PatientId.Should().Be(dto.PatientId);
            entity.DoctorId.Should().Be(dto.DoctorId);
            entity.DateTime.Should().Be(dto.DateTime);
            entity.IsConfirmed.Should().BeFalse();
        }

        [Fact]
        public void ToDto_ForSimpleModels_ShouldMapCorrectly()
        {
            // Arrange
            var spec = new Specialization { Id = Guid.NewGuid(), Name = "Cardio", IsActive = true };
            var office = new Office { Id = "Off-1", Address = "Street 1", IsActive = true };

            // Act
            var specDto = _mapper.ToDto(spec);
            var officeDto = _mapper.ToDto(office);

            // Assert
            specDto.Name.Should().Be(spec.Name);
            officeDto.Address.Should().Be(office.Address);
        }
    }
}
using FluentValidation.TestHelper;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Validators;

namespace InnoClinic.Appointments.UnitTests.Validators
{
    public class AppointmentRequestValidatorTests
    {
        private readonly AppointmentRequestValidator _validator = new();

        [Fact]
        public void Validate_WhenPatientIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { PatientId = Guid.Empty };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PatientId)
                  .WithErrorMessage("Sign in to make an appointment");
        }

        [Fact]
        public void Validate_WhenSpecializationIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { SpecializationId = Guid.Empty };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SpecializationId)
                  .WithErrorMessage("Please, choose the specialization");
        }

        [Fact]
        public void Validate_WhenDoctorIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { DoctorId = Guid.Empty };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DoctorId)
                  .WithErrorMessage("Please, choose the doctor");
        }

        [Fact]
        public void Validate_WhenServiceIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { ServiceId = Guid.Empty };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ServiceId)
                  .WithErrorMessage("Please, choose the service");
        }

        [Fact]
        public void Validate_WhenOfficeIdIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { OfficeId = "" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OfficeId)
                  .WithErrorMessage("Please, choose the office");
        }

        [Fact]
        public void Validate_WhenDateTimeIsDefault_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { DateTime = default };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTime)
                  .WithErrorMessage("Please, choose the date and time");
        }

        [Fact]
        public void Validate_WhenDateTimeIsInPast_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { DateTime = DateTime.UtcNow.AddHours(-1) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTime)
                  .WithErrorMessage("Appointment date must be in the future");
        }

        [Fact]
        public void Validate_WhenDateTimeIsNow_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { DateTime = DateTime.UtcNow.AddSeconds(-1) };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DateTime);
        }

        [Fact]
        public void Validate_WhenModelIsValid_ShouldNotHaveAnyValidationErrors()
        {
            // Arrange
            var model = CreateValidModel();

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        private AppointmentRequestDTO CreateValidModel()
        {
            return new AppointmentRequestDTO(
                PatientId: Guid.NewGuid(),
                DoctorId: Guid.NewGuid(),
                ServiceId: Guid.NewGuid(),
                SpecializationId: Guid.NewGuid(),
                OfficeId: "office-uuid-123",
                DateTime: DateTime.UtcNow.AddDays(1)
            );
        }
    }
}
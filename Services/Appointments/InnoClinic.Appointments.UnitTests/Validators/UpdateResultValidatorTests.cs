using FluentValidation.TestHelper;
using InnoClinic.Appointments.Application.DTOs;
using InnoClinic.Appointments.Application.Validators;

namespace InnoClinic.Appointments.UnitTests.Validators
{
    public class UpdateResultValidatorTests
    {
        private readonly UpdateResultValidator _validator = new();

        [Fact]
        public void Validate_WhenComplaintsIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { Complaints = "" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Complaints)
                  .WithErrorMessage("Please, enter the complaints");
        }

        [Fact]
        public void Validate_WhenConclusionIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { Conclusion = "" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Conclusion)
                  .WithErrorMessage("Please, enter the conclusion");
        }

        [Fact]
        public void Validate_WhenDiagnosisIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { Diagnosis = "" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Diagnosis)
                  .WithErrorMessage("Please, enter the diagnosis");
        }

        [Fact]
        public void Validate_WhenRecommendationsIsEmpty_ShouldHaveValidationError()
        {
            // Arrange
            var model = CreateValidModel() with { Recommendations = "" };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Recommendations)
                  .WithErrorMessage("Please, enter the recommendations");
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

        private UpdateResultRequestDTO CreateValidModel()
        {
            return new UpdateResultRequestDTO(
                Complaints: "Patient reported improved breathing",
                Conclusion: "Recovery is on track",
                Diagnosis: "Acute bronchitis (resolving)",
                Recommendations: "Continue current medication for 3 more days"
            );
        }
    }
}
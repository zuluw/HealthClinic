using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;

namespace InnoClinic.Profiles.Application.Validators
{
    public class DoctorRequestValidator : AbstractValidator<DoctorRequestDTO>
    {
        public DoctorRequestValidator()
        {
            RuleFor(x => x.FirstName)
              .NotEmpty().WithMessage("First name must not be empty")
              .MaximumLength(50).WithMessage("First name must not be more than 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name must not be empty")
                .MaximumLength(50).WithMessage("Last name must not be more than 50 characters");

            RuleFor(x => x.MiddleName)
                .MaximumLength(50).WithMessage("Middle name must not be more than 50 characters");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateOnly.FromDateTime(DateTime.Now.AddYears(-18)))
                .WithMessage("Doctor must be at least 18 years old");

            RuleFor(x => x.CareerStartDate)
                .NotEmpty()
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("Career start date cannot be in the future");

            RuleFor(x => x.SpecializationIds)
                .NotEmpty().WithMessage("At least one specialization is required");

            RuleFor(x => x.OfficeId).NotEmpty();
        }
    }
}
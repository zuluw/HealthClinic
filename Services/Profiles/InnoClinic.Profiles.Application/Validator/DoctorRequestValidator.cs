using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;

namespace InnoClinic.Profiles.Application.Validators
{
    public class DoctorRequestValidator : AbstractValidator<DoctorRequestDTO>
    {
        public DoctorRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);

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
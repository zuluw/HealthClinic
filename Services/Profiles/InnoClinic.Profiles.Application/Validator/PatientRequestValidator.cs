using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;

namespace InnoClinic.Profiles.Application.Validators
{
    public class PatientRequestValidator : AbstractValidator<PatientRequestDTO>
    {
        public PatientRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("The First name must not be empty")
                .MaximumLength(50).WithMessage("The First name must not be more than 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("The Last name must not be empty")
                .MaximumLength(50).WithMessage("The Last name must not be more than 50 characters");

            RuleFor(x => x.MiddleName)
                .MaximumLength(50).WithMessage("The Middle name must not be more than 50 characters");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateOnly.FromDateTime(DateTime.Now))
                .WithMessage("The date of birth cannot be in the future");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("The phone number must not be empty")
                .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format. Example: +375291234567");
        }
    }
}
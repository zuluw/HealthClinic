using FluentValidation;
using InnoClinic.Offices.Application.DTOs;

namespace InnoClinic.Offices.Application.Validators;

public class OfficeValidator : AbstractValidator<OfficeRequestDTO>
{
    public OfficeValidator()
    {
        RuleFor(o => o.Address)
            .NotEmpty().WithMessage("Address must not be empty")
            .MaximumLength(250).WithMessage("Address is too long");

        RuleFor(o => o.RegistryPhoneNumber)
            .NotEmpty().WithMessage("Phone number must not be empty")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format. Example: +375291234567");

        RuleFor(o => o.IsActive)
            .NotNull().WithMessage("Status must be specified");
    }
}
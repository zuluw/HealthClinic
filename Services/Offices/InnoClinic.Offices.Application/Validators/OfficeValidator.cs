using FluentValidation;
using InnoClinic.Offices.Application.DTOs;

namespace InnoClinic.Offices.Application.Validators;

public class OfficeValidator : AbstractValidator<OfficeDto>
{
    public OfficeValidator()
    {
        RuleFor(o => o.Address)
            .NotEmpty().WithMessage("Address cannot be empty")
            .MaximumLength(250).WithMessage("Address is too long");

        RuleFor(o => o.RegistryPhoneNumber)
            .NotEmpty().WithMessage("Phone number cannot be empty")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Invalid phone number format. Use e.g. +375291234567");

        RuleFor(o => o.IsActive)
            .NotNull().WithMessage("Status must be specified");
    }
}
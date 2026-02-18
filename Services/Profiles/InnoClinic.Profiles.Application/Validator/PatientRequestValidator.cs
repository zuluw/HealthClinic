using FluentValidation;
using InnoClinic.Profiles.Application.DTOs;

namespace InnoClinic.Profiles.Application.Validators
{
    public class PatientRequestValidator : AbstractValidator<PatientRequestDTO>
    {
        public PatientRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.PhoneNumber).NotEmpty().MinimumLength(10);
            // TODO
        }
    }
}
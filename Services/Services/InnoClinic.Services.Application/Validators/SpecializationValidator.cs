using FluentValidation;
using InnoClinic.Services.Application.DTOs;

namespace InnoClinic.Services.Application.Validators
{
    public class SpecializationValidator : AbstractValidator<SpecializationRequestDTO>
    {
        public SpecializationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name must not be empty")
                .MaximumLength(150);

            RuleFor(x => x.Services)
                .NotEmpty().WithMessage("At least one service is required");

            RuleForEach(x => x.Services).ChildRules(service =>
            {
                service.RuleFor(s => s.ServiceName)
                    .NotEmpty().WithMessage("Service name is required")
                    .MaximumLength(200);

                service.RuleFor(s => s.Price)
                    .GreaterThanOrEqualTo(0).WithMessage("Price must not be negative");

                service.RuleFor(s => s.Category)
                    .IsInEnum().WithMessage("Invalid service category");
            });
        }
    }
}
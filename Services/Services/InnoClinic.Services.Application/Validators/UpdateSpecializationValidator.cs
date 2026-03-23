using FluentValidation;
using InnoClinic.Services.Application.DTOs;

namespace InnoClinic.Services.Application.Validators
{
    public class UpdateSpecializationValidator : AbstractValidator<UpdateSpecializationRequestDTO>
    {
        public UpdateSpecializationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please, enter the name")
                .MaximumLength(150);

            RuleFor(x => x.Services)
                .NotEmpty().WithMessage("At least one service is required");

            RuleForEach(x => x.Services).ChildRules(service =>
            {
                service.RuleFor(s => s.ServiceName)
                    .NotEmpty().WithMessage("Please, enter the name")
                    .MaximumLength(200);

                service.RuleFor(s => s.Price)
                    .NotEmpty().WithMessage("Please, enter the price")
                    .GreaterThan(0).WithMessage("You've entered an invalid price"); 

                service.RuleFor(s => s.Category)
                    .IsInEnum().WithMessage("Please, choose the service category");
            });
        }
    }
}
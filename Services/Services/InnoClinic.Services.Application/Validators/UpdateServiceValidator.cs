using FluentValidation;
using InnoClinic.Services.Application.DTOs;

namespace InnoClinic.Services.Application.Validators
{
    public class UpdateServiceValidator : AbstractValidator<UpdateServiceRequestDTO>
    {
        public UpdateServiceValidator()
        {
            RuleFor(x => x.ServiceName)
                .NotEmpty().WithMessage("Please, enter the name")
                .MaximumLength(200);

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Please, enter the price")
                .GreaterThan(0).WithMessage("You've entered an invalid price");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Please, choose the service category");
        }
    }
}
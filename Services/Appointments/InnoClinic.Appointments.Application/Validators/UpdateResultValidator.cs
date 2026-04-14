using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;

namespace InnoClinic.Appointments.Application.Validators
{
    public class UpdateResultValidator : AbstractValidator<UpdateResultRequestDTO>
    {
        public UpdateResultValidator()
        {
            RuleFor(x => x.Complaints)
                .NotEmpty().WithMessage("Please, enter the complaints");

            RuleFor(x => x.Conclusion)
                .NotEmpty().WithMessage("Please, enter the conclusion");

            RuleFor(x => x.Recommendations)
                .NotEmpty().WithMessage("Please, enter the recommendations");
        }
    }
}
using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;

namespace InnoClinic.Appointments.Application.Validators
{
    public class ResultRequestValidator : AbstractValidator<ResultRequestDTO>
    {
        public ResultRequestValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty().WithMessage("Appointment ID is required");

            RuleFor(x => x.Complaints)
                .NotEmpty().WithMessage("Please, enter the complaints");

            RuleFor(x => x.Conclusion)
                .NotEmpty().WithMessage("Please, enter the conclusion");

            RuleFor(x => x.Diagnosis)
                .NotEmpty().WithMessage("Please, enter the diagnosis");

            RuleFor(x => x.Recommendations)
                .NotEmpty().WithMessage("Please, enter the recommendations");
        }
    }
}
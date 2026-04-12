using FluentValidation;
using InnoClinic.Appointments.Application.DTOs;

namespace InnoClinic.Appointments.Application.Validators
{
    public class AppointmentRequestValidator : AbstractValidator<AppointmentRequestDTO>
    {
        public AppointmentRequestValidator()
        {
            RuleFor(x => x.PatientId)
                .NotEmpty().WithMessage("Sign in to make an appointment");

            RuleFor(x => x.SpecializationId)
                .NotEmpty().WithMessage("Please, choose the specialization");

            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("Please, choose the doctor");

            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("Please, choose the service");

            RuleFor(x => x.OfficeId)
                .NotEmpty().WithMessage("Please, choose the office");

            RuleFor(x => x.DateTime)
                .NotEmpty().WithMessage("Please, choose the date and time")
                .GreaterThan(DateTime.UtcNow).WithMessage("Appointment date must be in the future");
        }
    }
}
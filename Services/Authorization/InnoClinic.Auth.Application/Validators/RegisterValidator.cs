using FluentValidation;
using InnoClinic.Auth.Application.DTOs;

namespace InnoClinic.Auth.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterRequestDTO>
    {
        public RegisterValidator()
        {
            RuleFor(u => u.UserName)
                .NotEmpty().WithMessage("Username must not be empty")
                .Length(5, 20).WithMessage("Username must be between 5 and 20");

            RuleFor(x => x.FirstName)
               .NotEmpty().WithMessage("First name must not be empty")
               .MaximumLength(50).WithMessage("First name must not be more than 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name must not be empty")
                .MaximumLength(50).WithMessage("Last name must not be more than 50 characters");

            RuleFor(u => u.Password)
                .MinimumLength(10).WithMessage("Password minimum length is 10")
                .Matches(@"\d+").WithMessage("Password must contain digits")
                .Matches(@"[A-Z]+").WithMessage("Password must contain capital letter")
                .Matches(@"[!@#$%^&*()_+';.,№:?]").WithMessage("Password must contain special character");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("Email address must not be empty")
                .EmailAddress().WithMessage("Incorrect email address")
                .MaximumLength(200).WithMessage("Email address must not be more than 200 characters");

        }
    }
}
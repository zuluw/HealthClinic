using FluentValidation;
using InnoClinic.Auth.Application.DTOs;

namespace InnoClinic.Auth.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterValidator() {
            RuleFor(u => u.UserName)
                .Length(5, 20).WithMessage("The nickname must be between 5 and 20!");

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("The first name cannot be empty!");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("The last name cannot be empty!");

            RuleFor(u => u.Password)
                .MinimumLength(10).WithMessage("The password minimum length is 10!")
                .Matches(@"\d+").WithMessage("The passowrd must contain digits!")
                .Matches(@"[A-Z]+").WithMessage("The password must contain capital letter!")
                .Matches(@"[!@#$%^&*()_+';.,№:?]").WithMessage("The password must contain special character!");

            RuleFor(u => u.Email)
                .EmailAddress().WithMessage("Incorrect email address!");

        }
    }
}

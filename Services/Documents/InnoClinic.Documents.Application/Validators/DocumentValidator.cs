using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace InnoClinic.Documents.Application.Validators
{
    public class DocumentValidator : AbstractValidator<IFormFile>
    {
        private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSize = 5 * 1024 * 1024;

        public DocumentValidator()
        {
            RuleFor(x => x.Length)
                .NotNull()
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage("File size must not exceed 5 MB");

            RuleFor(x => x.FileName)
                .Must(HaveAllowedExtension)
                .WithMessage($"Only following extensions are allowed: {string.Join(", ", _allowedExtensions)}");
        }

        private bool HaveAllowedExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return _allowedExtensions.Contains(extension);
        }
    }
}
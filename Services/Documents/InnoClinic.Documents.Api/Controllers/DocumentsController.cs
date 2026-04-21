using FluentValidation;
using InnoClinic.Documents.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InnoClinic.Documents.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController(IDocumentService documentService, IValidator<IFormFile> validator) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
        {
            var validationResult = await validator.ValidateAsync(file, ct);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            using var stream = file.OpenReadStream();
            var result = await documentService.UploadAsync(stream, file.FileName, file.ContentType, ct);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Download(Guid id, CancellationToken ct)
        {
            var (stream, contentType, fileName) = await documentService.DownloadAsync(id, ct);

            return File(stream, contentType, fileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            await documentService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
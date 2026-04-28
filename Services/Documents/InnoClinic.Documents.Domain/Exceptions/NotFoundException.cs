namespace InnoClinic.Documents.Domain.Exceptions
{
    public class NotFoundException(string message) : BaseException(message, 404);
}
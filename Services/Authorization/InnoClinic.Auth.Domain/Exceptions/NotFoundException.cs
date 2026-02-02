namespace InnoClinic.Auth.Domain.Exceptions
{
    public class NotFoundException(string message) : BaseException(message, 404);
}
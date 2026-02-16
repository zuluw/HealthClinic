namespace InnoClinic.Profiles.Domain.Exceptions
{
    public class NotFoundException(string message) : BaseException(message, 404);
}
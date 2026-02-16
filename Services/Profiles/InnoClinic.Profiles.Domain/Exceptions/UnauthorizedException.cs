namespace InnoClinic.Profiles.Domain.Exceptions
{
    public class UnauthorizedException(string message) : BaseException(message, 404);
}
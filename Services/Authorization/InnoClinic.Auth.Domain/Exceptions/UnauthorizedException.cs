namespace InnoClinic.Auth.Domain.Exceptions
{
    public class UnauthorizedException(string message) : BaseException(message, 401);
}
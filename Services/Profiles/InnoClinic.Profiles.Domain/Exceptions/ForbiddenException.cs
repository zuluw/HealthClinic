namespace InnoClinic.Profiles.Domain.Exceptions
{
    public class ForbiddenException(string message) : BaseException(message, 404);
}
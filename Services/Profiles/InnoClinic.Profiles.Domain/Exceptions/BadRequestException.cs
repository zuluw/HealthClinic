namespace InnoClinic.Profiles.Domain.Exceptions
{
    public class BadRequestException(string message) : BaseException(message, 400);
}
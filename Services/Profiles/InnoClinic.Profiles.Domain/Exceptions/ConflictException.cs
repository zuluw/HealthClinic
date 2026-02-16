namespace InnoClinic.Profiles.Domain.Exceptions
{
    public class ConflictException(string message) : BaseException(message, 404);
}
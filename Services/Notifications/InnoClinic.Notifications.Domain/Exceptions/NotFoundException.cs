namespace InnoClinic.Notifications.Domain.Exceptions
{
    public class NotFoundException(string message) : BaseException(message, 404);
}
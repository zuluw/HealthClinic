namespace InnoClinic.Notifications.Domain.Exceptions
{
    public class BadRequestException(string message) : BaseException(message, 400);
}
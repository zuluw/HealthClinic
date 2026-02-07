namespace InnoClinic.Offices.Domain.Exceptions
{
    public class NotFoundException(string message) : BaseException(message, 404);
}
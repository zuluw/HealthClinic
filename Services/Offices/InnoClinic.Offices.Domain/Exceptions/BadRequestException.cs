namespace InnoClinic.Offices.Domain.Exceptions
{
    public class BadRequestException(string message) : BaseException(message, 400);
}

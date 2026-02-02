namespace InnoClinic.Auth.Domain.Exceptions;

public class BadRequestException(string message) : BaseException(message, 400);
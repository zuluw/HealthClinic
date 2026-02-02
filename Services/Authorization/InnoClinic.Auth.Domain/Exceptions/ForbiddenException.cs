namespace InnoClinic.Auth.Domain.Exceptions;

public class ForbiddenException(string message) : BaseException(message, 403);
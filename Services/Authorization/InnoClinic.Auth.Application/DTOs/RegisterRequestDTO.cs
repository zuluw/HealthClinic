namespace InnoClinic.Auth.Application.DTOs
{
    public record RegisterRequestDTO(
        string UserName,
        string Password,
        string FirstName,
        string LastName,
        string Email
    );
}
namespace InnoClinic.Auth.Application.DTOs
{
    public record RegisterDTO(
        string UserName,
        string Password,
        string FirstName,
        string LastName,
        string Email
    );
}
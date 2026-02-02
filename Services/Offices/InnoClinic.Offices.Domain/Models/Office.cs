namespace InnoClinic.Offices.Domain.Models;

public class Office
{
    public string Id { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
    public string RegistryPhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
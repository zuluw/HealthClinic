namespace InnoClinic.Offices.Domain.Models;

public class Office
{
    public string Id { get; init; } = string.Empty;

    public required string Address { get; set; } 
    public required string RegistryPhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
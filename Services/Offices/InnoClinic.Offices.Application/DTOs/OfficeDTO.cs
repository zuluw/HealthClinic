namespace InnoClinic.Offices.Application.DTOs;

public class OfficeDto
{
    public string Address { get; set; } = string.Empty;
    public string RegistryPhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
namespace InnoClinic.Appointments.Domain.Models
{
    public class Office
    {
        public required string Id { get; init; }
        public required string Address { get; set; }
        public bool IsActive { get; set; }
    }
}
namespace InnoClinic.Profiles.Domain.Models
{
    public class Specialization
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
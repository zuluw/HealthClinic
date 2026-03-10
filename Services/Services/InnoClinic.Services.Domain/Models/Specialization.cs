using InnoClinic.Services.Domain.Models;

namespace InnoClinic.Services.Domain.Models
{
    public class Specialization
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
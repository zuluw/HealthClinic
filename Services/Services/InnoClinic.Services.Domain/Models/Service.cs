using InnoClinic.Services.Domain.Enums;

namespace InnoClinic.Services.Domain.Models
{
    public class Service
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string ServiceName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public ServiceCategory Category { get; set; }

        public Guid SpecializationId { get; set; }
        public virtual Specialization Specialization { get; set; } = null!;
    }
}
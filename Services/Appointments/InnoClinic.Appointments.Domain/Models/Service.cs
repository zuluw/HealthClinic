using InnoClinic.Appointments.Domain.Enums;

namespace InnoClinic.Appointments.Domain.Models
{
    public class Service
    {
        public Guid Id { get; init; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public ServiceCategory Category { get; set; }

        public Guid SpecializationId { get; set; }
        public virtual Specialization Specialization { get; set; } = null!;
    }
}
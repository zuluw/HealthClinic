namespace InnoClinic.Common.Contracts.Events
{
    public record ServiceCreatedEvent(Guid Id, string Name, decimal Price, string Category, Guid SpecializationId);
    public record ServiceUpdatedEvent(Guid Id, string Name, decimal Price, bool IsActive);
    public record ServiceStatusChangedEvent(Guid Id, bool IsActive);
    public record ServiceDeletedEvent(Guid Id);
}

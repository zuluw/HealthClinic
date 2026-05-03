namespace InnoClinic.Common.Contracts.Events
{
    public record ServiceCreatedEvent(Guid Id, string Name, decimal Price, string Category, Guid SpecializationId);
    public record ServiceUpdatedEvent(Guid Id, string Name, decimal Price, bool IsActive);
    public record ServiceStatusChangedEvent(Guid Id, bool IsActive);
    public record ServiceDeletedEvent(Guid Id);
    public record AppointmentCreatedEvent(
        Guid Id,
        string PatientFirstName,
        string PatientLastName,
        string? PatientMiddleName,
        string PatientEmail,
        string DoctorFirstName,
        string DoctorLastName,
        string? DoctorMiddleName,
        string ServiceName,
        DateTime DateTime);
    public record ResultCreatedEvent(
        Guid AppointmentId,
        string PatientEmail,
        string PatientFullName,
        string DoctorFullName,
        string ServiceName,
        DateTime DateTime,
        string Complaints,
        string Conclusion,
        string Diagnosis,
        string Recommendations);
    public record ResultUpdatedEvent(
        Guid AppointmentId,
        string PatientEmail,
        string PatientFullName,
        string DoctorFullName,
        string ServiceName,
        DateTime DateTime,
        string Complaints,
        string Conclusion,
        string Diagnosis,
        string Recommendations);
}

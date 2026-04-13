namespace InnoClinic.Appointments.Application.DTOs
{
    public record ResultResponseDTO(
        Guid Id,
        DateTime Date,
        string PatientFullName,
        DateOnly PatientDateOfBirth,
        string DoctorFullName,
        string DoctorSpecialization,
        string ServiceName,
        string Complaints,
        string Conclusion,
        string Recommendations
    );
}
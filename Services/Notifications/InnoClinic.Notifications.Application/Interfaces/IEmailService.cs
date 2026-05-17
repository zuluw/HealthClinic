namespace InnoClinic.Notifications.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAppointmentAsync(string toEmail, string patientName, DateTime date, string serviceName, string doctorName, CancellationToken ct);
    }
}
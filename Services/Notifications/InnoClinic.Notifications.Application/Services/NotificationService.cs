using InnoClinic.Common.Contracts.Events;
using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Domain.Enums;
using InnoClinic.Notifications.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InnoClinic.Notifications.Application.Services
{
    public class NotificationService(INotificationRepository repository, IEmailService emailService, IConfiguration configuration, ILogger<NotificationService> logger) : INotificationService
    {
        public async Task SendDailyRemindersAsync(CancellationToken ct)
        {
            var daysStart = configuration.GetValue<int>("NotificationSettings:ReminderDaysStart");
            var windowDays = configuration.GetValue<int>("NotificationSettings:ReminderWindowDays");

            logger.LogInformation("Hangfire recurring job started: checking for upcoming appointments...");

            var startDate = DateTime.UtcNow.AddDays(daysStart).Date;
            var endDate = startDate.AddDays(windowDays);

            var notifications = await repository.GetPendingNotificationsAsync(startDate, endDate, ct);

            logger.LogInformation("Found {Count} pending notifications for the period {Start} to {End}", notifications.Count, startDate, endDate);

            foreach (var notification in notifications)
            {
                try
                {
                    await emailService.SendAppointmentAsync(
                        notification.PatientEmail,
                        notification.PatientFullName,
                        notification.AppointmentDateTime,
                        notification.ServiceName,
                        notification.DoctorFullName,
                        ct);

                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                    repository.Update(notification);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while sending notification for appointment {Id}", notification.AppointmentId);
                    notification.Status = NotificationStatus.Failed;
                }
            }

            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Hangfire recurring job finished successfully.");
        }

        public async Task ProcessAppointmentCreatedAsync(AppointmentCreatedEvent data, CancellationToken ct)
        {
            logger.LogInformation("Sync: Saving appointment {Id} for future reminder", data.Id);

            var notification = new Notification
            {
                AppointmentId = data.Id,
                PatientFullName = $"{data.PatientLastName} {data.PatientFirstName} {data.PatientMiddleName}".Trim(),
                PatientEmail = data.PatientEmail,
                DoctorFullName = $"{data.DoctorLastName} {data.DoctorFirstName} {data.DoctorMiddleName}".Trim(),
                ServiceName = data.ServiceName,
                AppointmentDateTime = data.DateTime
            };

            repository.Add(notification);
            await repository.SaveChangesAsync(ct);
        }

        public async Task ProcessResultCreatedAsync(ResultCreatedEvent data, CancellationToken ct)
        {
            logger.LogInformation("Processing result creation for appointment {Id}", data.AppointmentId);
            var subject = configuration["EmailSettings:Subject"];
            await GenerateResultAsync(data, subject!, ct);
        }

        public async Task ProcessResultUpdatedAsync(ResultUpdatedEvent data, CancellationToken ct)
        {
            logger.LogInformation("Processing result update for appointment {Id}", data.AppointmentId);
            var subject = configuration["EmailSettings:Subject"];

            var eventData = new ResultCreatedEvent(
                data.AppointmentId, data.PatientEmail, data.PatientFullName, data.DoctorFullName,
                data.ServiceName, data.DateTime, data.Complaints, data.Conclusion, data.Diagnosis, data.Recommendations);

            await GenerateResultAsync(eventData, subject!, ct);
        }

        private async Task GenerateResultAsync(ResultCreatedEvent data, string subject, CancellationToken ct)
        {
            var pdfBytes = GenerateResultPdf(data);
            var fileName = $"Result_{data.DateTime:yyyyMMdd}.pdf";
            var body = $"Hello, {data.PatientFullName}! Your medical result for {data.ServiceName} is attached.";

            await emailService.SendAttachmentAsync(data.PatientEmail, subject, body, pdfBytes, fileName, ct);
        }

        private byte[] GenerateResultPdf(ResultCreatedEvent result)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Header().Text("Medical Report").FontSize(24).Bold().FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120);
                            columns.RelativeColumn();
                        });

                        AddRow(table, "Patient:", result.PatientFullName);
                        AddRow(table, "Doctor:", result.DoctorFullName);
                        AddRow(table, "Service:", result.ServiceName);
                        AddRow(table, "Date:", result.DateTime.ToString("f"));
                        AddRow(table, "Diagnosis:", result.Diagnosis);
                        AddRow(table, "Complaints:", result.Complaints);
                        AddRow(table, "Conclusion:", result.Conclusion);
                        AddRow(table, "Recommendations:", result.Recommendations);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on: ");
                        x.Span(DateTime.Now.ToString("f"));
                    });
                });
            }).GeneratePdf();
        }

        private void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell().Text(label).Bold();
            table.Cell().Text(value);
        }
    }
}
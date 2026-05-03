using InnoClinic.Notifications.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace InnoClinic.Notifications.Infrastructure.ExternalServices
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendAppointmentAsync(string toEmail, string patientName, DateTime date, string serviceName, string doctorName, CancellationToken ct)
        {
            var smtpHost = configuration["EmailSettings:Host"];
            var smtpPort = configuration.GetValue<int>("EmailSettings:Port");
            var fromEmail = configuration["EmailSettings:SenderEmail"];
            var fromName = configuration["EmailSettings:SenderName"];
            var subject = configuration["EmailSettings:Subject"];

            logger.LogInformation("Attempting to send SMTP email to {Email}", toEmail);

            var body = $@"Hello, {patientName}!
                This is a reminder for your upcoming appointment:
                Date: {date:dd.MM.yyyy}
                Time: {date:HH:mm}
                Service: {serviceName}
                Doctor: {doctorName}
                We look forward to seeing you at our clinic!";

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage, ct);

                logger.LogInformation("Email successfully processed for {Email}. Body details: Service: {Service}, Doctor: {Doctor}", toEmail, serviceName, doctorName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Email} via SMTP host {Host}", toEmail, smtpHost);
                throw;
            }
        }

        public async Task SendAttachmentAsync(string toEmail, string subject, string body, byte[] pdfContent, string fileName, CancellationToken ct)
        {
            var smtpHost = configuration["EmailSettings:Host"];
            var smtpPort = configuration.GetValue<int>("EmailSettings:Port");
            var fromEmail = configuration["EmailSettings:SenderEmail"];
            var fromName = configuration["EmailSettings:SenderName"];

            logger.LogInformation("Attempting to send result email with attachment to {Email}", toEmail);

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort);
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                using var ms = new MemoryStream(pdfContent);
                var attachment = new Attachment(ms, fileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);

                await client.SendMailAsync(mailMessage, ct);

                logger.LogInformation("Result email with attachment successfully sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send result email to {Email}", toEmail);
                throw;
            }
        }
    }
}
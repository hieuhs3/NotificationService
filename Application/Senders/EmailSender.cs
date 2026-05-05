using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Senders;

public class EmailSender : INotificationSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(NotificationEvent e)
    {
        if (e.Channel == "Email" || e.Channel == "All")
        {
            var host = _configuration["SmtpSettings:Host"];
            var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "1025");
            var username = _configuration["SmtpSettings:Username"];
            var password = _configuration["SmtpSettings:Password"];
            var from = _configuration["SmtpSettings:From"] ?? "no-reply@synapse.com";

            // In a real scenario, the recipient email would be fetched using e.UserId from a User profile database.
            // For demonstration, we send it to a mock address based on the UserId.
            var toEmail = $"{e.UserId}@mockuser.com";

            using var client = new SmtpClient(host, port);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new System.Net.NetworkCredential(username, password);
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(from),
                Subject = $"Notification: {e.Type}",
                Body = e.Message,
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail} for Event {EventId}", toEmail, e.EventId);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail} for Event {EventId}", toEmail, e.EventId);
            }
        }
    }
}

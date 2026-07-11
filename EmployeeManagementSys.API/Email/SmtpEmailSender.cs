using System.Net;
using System.Net.Mail;
using EmployeeManagementSys.BL;
using Microsoft.Extensions.Options;

namespace EmployeeManagementSys.API.Email
{
    /// <summary>
    /// SMTP implementation of <see cref="IEmailSender"/> using the built-in
    /// System.Net.Mail client. Dependency-free and sufficient for the local
    /// smtp4dev catcher and a plain SMTP relay. See
    /// docs/agdr/AgDR-0005-email-sender.md — MailKit is the recommended
    /// hardening for a production provider.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl
            };
            if (!string.IsNullOrWhiteSpace(_settings.User))
            {
                client.Credentials = new NetworkCredential(_settings.User, _settings.Password);
            }

            using var message = new MailMessage(_settings.From, to, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Sent '{Subject}' email to {To}", subject, to);
        }
    }
}

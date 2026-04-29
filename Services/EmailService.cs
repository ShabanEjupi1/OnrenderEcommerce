using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ProjectTemplate.Services
{
    public interface IEmailService
    {
        Task SendNoReplyEmailAsync(string toEmail, string subject, string body, bool isSq = false);
        Task SendContactMessageAsync(string fromName, string fromEmail, string message, bool isSq = false);
    }

    public class EmailService : IEmailService
    {
        private readonly string _brandName;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _brandName = configuration["Branding:BrandName"] ?? "YourBrand";
            _smtpHost = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST")
                ?? configuration["Email:Smtp:Host"]
                ?? string.Empty;
            _smtpUser = Environment.GetEnvironmentVariable("EMAIL_SMTP_USER")
                ?? configuration["Email:Smtp:User"]
                ?? string.Empty;

            var portValue = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT")
                ?? configuration["Email:Smtp:Port"];
            _smtpPort = int.TryParse(portValue, out var port) ? port : 587;

            var pass = Environment.GetEnvironmentVariable("EMAIL_SMTP_PASS")
                ?? Environment.GetEnvironmentVariable("APP_PASSWORD")
                ?? configuration["Email:Smtp:Pass"]
                ?? configuration["APP_PASSWORD"]
                ?? string.Empty;
            _smtpPass = pass.Replace(" ", "");

            var sslValue = Environment.GetEnvironmentVariable("EMAIL_SMTP_ENABLE_SSL")
                ?? configuration["Email:Smtp:EnableSsl"];
            _enableSsl = !string.IsNullOrWhiteSpace(sslValue)
                ? bool.TryParse(sslValue, out var enableSsl) && enableSsl
                : true;
        }

        public async Task SendNoReplyEmailAsync(string toEmail, string subject, string body, bool isSq = false)
        {
            if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPass)) return;

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser, isSq ? $"{_brandName} Mos Kthe PÃ«rgjigje" : $"{_brandName} No-Reply"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }

        public async Task SendContactMessageAsync(string fromName, string fromEmail, string message, bool isSq = false)
        {
            if (string.IsNullOrEmpty(_smtpHost) || string.IsNullOrEmpty(_smtpUser) || string.IsNullOrEmpty(_smtpPass)) return;

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = _enableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser, isSq ? $"{_brandName} Kontakt" : $"{_brandName} Website Contact"),
                Subject = isSq ? $"Mesazh i Ri Kontakti nga {fromName}" : $"New Contact Message from {fromName}",
                Body = isSq 
                    ? $"<b>Emri:</b> {fromName}<br/><b>Email:</b> {fromEmail}<br/><br/><b>Mesazhi:</b><br/>{message}"
                    : $"<b>Name:</b> {fromName}<br/><b>Email:</b> {fromEmail}<br/><br/><b>Message:</b><br/>{message}",
                IsBodyHtml = true
            };
            // Send to ourselves
            mailMessage.To.Add(_smtpUser);
            mailMessage.ReplyToList.Add(new MailAddress(fromEmail, fromName));

            await client.SendMailAsync(mailMessage);
        }
    }
}

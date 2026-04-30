using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProjectTemplate.Services
{
    public interface IEmailService
    {
        Task<bool> SendNoReplyEmailAsync(string toEmail, string subject, string body, bool isSq = false);
        Task<bool> SendContactMessageAsync(string fromName, string fromEmail, string message, bool isSq = false);
        Task<bool> SendSubscriptionConfirmationAsync(string toEmail, string? name, string unsubscribeToken, bool isSq = false);
        Task<bool> SendNewsletterAsync(IEnumerable<(string Email, string Token)> recipients, string subject, string htmlBody);
        Task<bool> SendOrderConfirmationAsync(string toEmail, string customerName, string orderNumber, decimal total, bool isSq = false);
        bool IsConfigured { get; }
    }

    /// <summary>
    /// SMTP email service for Enisi Center. Reads credentials from environment variables
    /// (EMAIL_SMTP_HOST, EMAIL_SMTP_USER, EMAIL_SMTP_PASS, EMAIL_SMTP_PORT, EMAIL_SMTP_ENABLE_SSL).
    /// Falls back to appsettings.json configuration.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly string _brandName;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly bool _enableSsl;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _brandName = configuration["Branding:BrandName"] ?? "Enisi Center";

            _smtpHost = GetEnv("EMAIL_SMTP_HOST")
                ?? configuration["Email:Smtp:Host"]
                ?? string.Empty;

            _smtpUser = GetEnv("EMAIL_SMTP_USER")
                ?? configuration["Email:Smtp:User"]
                ?? string.Empty;

            var rawPass = GetEnv("EMAIL_SMTP_PASS")
                ?? configuration["Email:Smtp:Pass"]
                ?? string.Empty;

            // Remove spaces from app passwords (Gmail 16-char passwords sometimes have spaces)
            _smtpPass = rawPass.Replace(" ", "");

            var portValue = GetEnv("EMAIL_SMTP_PORT") ?? configuration["Email:Smtp:Port"];
            _smtpPort = int.TryParse(portValue, out var port) ? port : 587;

            var sslValue = GetEnv("EMAIL_SMTP_ENABLE_SSL") ?? configuration["Email:Smtp:EnableSsl"];
            _enableSsl = string.IsNullOrWhiteSpace(sslValue)
                ? true
                : bool.TryParse(sslValue, out var ssl) && ssl;

            if (!string.IsNullOrEmpty(_smtpHost))
                _logger.LogInformation("[Email] SMTP configured: {Host}:{Port} SSL={Ssl} User={User}",
                    _smtpHost, _smtpPort, _enableSsl, _smtpUser);
            else
                _logger.LogWarning("[Email] SMTP not configured. EMAIL_SMTP_HOST is empty. Emails will be skipped.");
        }

        private static string? GetEnv(string key) =>
            Environment.GetEnvironmentVariable(key) is { Length: > 0 } val ? val : null;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_smtpHost) &&
            !string.IsNullOrWhiteSpace(_smtpUser) &&
            !string.IsNullOrWhiteSpace(_smtpPass);

        // ------------------------------------------------------------------
        // Core send helper
        // ------------------------------------------------------------------

        private SmtpClient CreateClient() => new SmtpClient(_smtpHost, _smtpPort)
        {
            EnableSsl = _enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtpUser, _smtpPass),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15_000
        };

        private async Task<bool> SendAsync(MailMessage msg)
        {
            if (!IsConfigured)
            {
                _logger.LogWarning("[Email] Skipping send — SMTP not configured. To={To} Subject={Subject}",
                    msg.To.ToString(), msg.Subject);
                return false;
            }

            try
            {
                using var client = CreateClient();
                await client.SendMailAsync(msg);
                _logger.LogInformation("[Email] Sent OK to {To} — {Subject}", msg.To.ToString(), msg.Subject);
                return true;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "[Email] SMTP error sending to {To}: {Message}", msg.To.ToString(), ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Email] Unexpected error sending to {To}", msg.To.ToString());
                return false;
            }
        }

        // ------------------------------------------------------------------
        // Public interface implementations
        // ------------------------------------------------------------------

        public async Task<bool> SendNoReplyEmailAsync(string toEmail, string subject, string body, bool isSq = false)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_smtpUser,
                    isSq ? $"{_brandName} Mos Kthe Përgjigje" : $"{_brandName} No-Reply"),
                Subject = subject,
                Body = WrapInTemplate(body, subject),
                IsBodyHtml = true
            };
            msg.To.Add(toEmail);
            return await SendAsync(msg);
        }

        public async Task<bool> SendContactMessageAsync(string fromName, string fromEmail, string message, bool isSq = false)
        {
            var subject = isSq
                ? $"Mesazh i Ri nga {fromName}"
                : $"New Contact Message from {fromName}";

            var bodyContent = isSq
                ? $"<p><b>Emri:</b> {System.Net.WebUtility.HtmlEncode(fromName)}</p>" +
                  $"<p><b>Email:</b> {System.Net.WebUtility.HtmlEncode(fromEmail)}</p>" +
                  $"<hr/><p><b>Mesazhi:</b></p><p>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}</p>"
                : $"<p><b>Name:</b> {System.Net.WebUtility.HtmlEncode(fromName)}</p>" +
                  $"<p><b>Email:</b> {System.Net.WebUtility.HtmlEncode(fromEmail)}</p>" +
                  $"<hr/><p><b>Message:</b></p><p>{System.Net.WebUtility.HtmlEncode(message).Replace("\n", "<br/>")}</p>";

            using var msg = new MailMessage
            {
                From = new MailAddress(_smtpUser,
                    isSq ? $"{_brandName} Kontakt" : $"{_brandName} Contact"),
                Subject = subject,
                Body = WrapInTemplate(bodyContent, subject),
                IsBodyHtml = true
            };
            msg.To.Add(_smtpUser); // forward to ourselves
            msg.ReplyToList.Add(new MailAddress(fromEmail, fromName));
            return await SendAsync(msg);
        }

        public async Task<bool> SendSubscriptionConfirmationAsync(
            string toEmail, string? name, string unsubscribeToken, bool isSq = false)
        {
            var displayName = string.IsNullOrWhiteSpace(name) ? toEmail : name;
            var unsubLink = $"https://enisicenter.tech/Home/Unsubscribe?token={unsubscribeToken}";

            var subject = isSq
                ? $"Mirë se u abonuat në {_brandName}!"
                : $"Welcome to {_brandName} Newsletter!";

            var bodyContent = isSq
                ? $"<h2 style='color:#4f46e5'>Faleminderit, {System.Net.WebUtility.HtmlEncode(displayName)}! 🎉</h2>" +
                  $"<p>Jeni abonuar me sukses në buletinin e <b>{_brandName}</b>.</p>" +
                  $"<p>Do të merrni lajme ekskluzive, oferta dhe promovime direkt në emailin tuaj.</p>" +
                  $"<hr/><p style='font-size:12px;color:#888'>Nëse dëshironi të çabonoheni: <a href='{unsubLink}'>klikoni këtu</a>.</p>"
                : $"<h2 style='color:#4f46e5'>Thank you, {System.Net.WebUtility.HtmlEncode(displayName)}! 🎉</h2>" +
                  $"<p>You have successfully subscribed to the <b>{_brandName}</b> newsletter.</p>" +
                  $"<p>You'll receive exclusive news, offers and promotions directly in your inbox.</p>" +
                  $"<hr/><p style='font-size:12px;color:#888'>To unsubscribe: <a href='{unsubLink}'>click here</a>.</p>";

            return await SendNoReplyEmailAsync(toEmail, subject, bodyContent, isSq);
        }

        public async Task<bool> SendNewsletterAsync(
            IEnumerable<(string Email, string Token)> recipients, string subject, string htmlBody)
        {
            if (!IsConfigured) return false;

            var sent = 0;
            var failed = 0;

            foreach (var (email, token) in recipients)
            {
                var unsubLink = $"https://enisicenter.tech/Home/Unsubscribe?token={token}";
                var fullBody = htmlBody +
                    $"<hr/><p style='font-size:11px;color:#aaa;text-align:center'>Çabonohuni: " +
                    $"<a href='{unsubLink}'>klikoni këtu</a> | {_brandName}</p>";

                using var msg = new MailMessage
                {
                    From = new MailAddress(_smtpUser, $"{_brandName} Newsletter"),
                    Subject = subject,
                    Body = WrapInTemplate(fullBody, subject),
                    IsBodyHtml = true
                };
                msg.To.Add(email);

                var ok = await SendAsync(msg);
                if (ok) sent++; else failed++;

                // Small delay to avoid rate limits
                await Task.Delay(150);
            }

            _logger.LogInformation("[Newsletter] Blast complete. Sent={Sent} Failed={Failed}", sent, failed);
            return failed == 0;
        }

        public async Task<bool> SendOrderConfirmationAsync(
            string toEmail, string customerName, string orderNumber, decimal total, bool isSq = false)
        {
            var subject = isSq
                ? $"Konfirmim i Porosisë #{orderNumber} — {_brandName}"
                : $"Order Confirmation #{orderNumber} — {_brandName}";

            var bodyContent = isSq
                ? $"<h2 style='color:#4f46e5'>Faleminderit për porosinë tuaj! 🛍️</h2>" +
                  $"<p>Përshëndetje <b>{System.Net.WebUtility.HtmlEncode(customerName)}</b>,</p>" +
                  $"<p>Porosia juaj <b>#{orderNumber}</b> u pranua me sukses.</p>" +
                  $"<p>Totali: <b style='color:#4f46e5;font-size:1.3em'>{total:F2} €</b></p>" +
                  $"<p>Ekipi ynë do të kontaktojë me ju shpejt për konfirmimin e dorëzimit.</p>" +
                  $"<p>Faleminderit që zgjodhët <b>{_brandName}</b>! ❤️</p>"
                : $"<h2 style='color:#4f46e5'>Thank you for your order! 🛍️</h2>" +
                  $"<p>Hello <b>{System.Net.WebUtility.HtmlEncode(customerName)}</b>,</p>" +
                  $"<p>Your order <b>#{orderNumber}</b> was successfully received.</p>" +
                  $"<p>Total: <b style='color:#4f46e5;font-size:1.3em'>{total:F2} €</b></p>" +
                  $"<p>Our team will contact you shortly to confirm delivery.</p>" +
                  $"<p>Thank you for choosing <b>{_brandName}</b>! ❤️</p>";

            return await SendNoReplyEmailAsync(toEmail, subject, bodyContent, isSq);
        }

        // ------------------------------------------------------------------
        // HTML template wrapper
        // ------------------------------------------------------------------

        private string WrapInTemplate(string content, string title)
        {
            return $"""
            <!DOCTYPE html>
            <html lang="sq">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
            <title>{System.Net.WebUtility.HtmlEncode(title)}</title></head>
            <body style="margin:0;padding:0;background:#f8fafc;font-family:'Segoe UI',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f8fafc;padding:40px 20px">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%">
                    <!-- Header -->
                    <tr><td style="background:linear-gradient(135deg,#4f46e5,#0ea5e9);border-radius:16px 16px 0 0;padding:32px 40px;text-align:center">
                      <h1 style="margin:0;color:white;font-size:24px;font-weight:800">{System.Net.WebUtility.HtmlEncode(_brandName)}</h1>
                      <p style="margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px">Dyqani juaj online i besueshëm</p>
                    </td></tr>
                    <!-- Body -->
                    <tr><td style="background:white;padding:40px;border-left:1px solid #e2e8f0;border-right:1px solid #e2e8f0;color:#0f172a;font-size:16px;line-height:1.7">
                      {content}
                    </td></tr>
                    <!-- Footer -->
                    <tr><td style="background:#0f172a;border-radius:0 0 16px 16px;padding:24px 40px;text-align:center">
                      <p style="margin:0;color:#64748b;font-size:12px">© {DateTime.Now.Year} {System.Net.WebUtility.HtmlEncode(_brandName)} · Podujevë, Kosovë</p>
                      <p style="margin:8px 0 0;color:#64748b;font-size:12px">info@enisicenter.tech · +383 45 594 549</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body></html>
            """;
        }
    }
}

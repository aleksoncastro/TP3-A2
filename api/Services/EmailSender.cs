using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace MediaMatch.Services
{
    public interface IEmailSender
    {
        Task SendPasswordResetCodeAsync(string email, string userName, string code);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetCodeAsync(string email, string userName, string code)
        {
            var host = _configuration["Email:Smtp:Host"];
            var port = _configuration.GetValue<int?>("Email:Smtp:Port") ?? 0;
            var username = _configuration["Email:Smtp:Username"];
            var password = _configuration["Email:Smtp:Password"];
            var from = _configuration["Email:From"];
            var enableSsl = _configuration.GetValue<bool?>("Email:Smtp:EnableSsl") ?? true;
            var displayName = _configuration["Email:DisplayName"] ?? "MediaMatch";
            var sender = _configuration["Email:Smtp:Sender"] ?? username ?? from;

            if (string.IsNullOrWhiteSpace(host) || port == 0 || string.IsNullOrWhiteSpace(sender))
            {
                _logger.LogWarning("SMTP configuration incomplete. Password reset email not sent.");
                return;
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);
            }

            var fromAddressValue = string.IsNullOrWhiteSpace(from) ? sender : from;

            using var message = new MailMessage
            {
                Subject = "Código para redefinição de senha",
                Body = $"Olá {userName},\n\nSeu código para redefinir a senha é: {code}. Ele expira em 15 minutos.\n\nSe você não solicitou, ignore este email.",
                IsBodyHtml = false
            };

            try
            {
                message.From = new MailAddress(fromAddressValue, displayName);
                message.Sender = new MailAddress(sender, displayName);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid email address configured for SMTP (From={From}, Sender={Sender}).", fromAddressValue, sender);
                return;
            }

            message.To.Add(email);
            message.ReplyToList.Add(message.From);

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("Password reset email sent to {Email}.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}.", email);
            }
        }
    }
}

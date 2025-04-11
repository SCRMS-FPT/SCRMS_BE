using MailKit.Security;
using MimeKit;


namespace Notification.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, Boolean isHtml);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly string _smtpPassword;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _smtpPassword = _config["Smtp:Password"]
                ?? throw new Exception("SMTP password not configured in appsettings.");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, Boolean isHtml)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Sports Court Management and Reservation System", _config["Smtp:Username"]));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart(isHtml ? "html" : "plain")
                {
                    Text = body
                };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["Smtp:Username"], _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }

}

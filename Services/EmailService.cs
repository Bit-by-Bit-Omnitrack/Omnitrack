using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace UserRoles.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = message
            };

            using var smtp = new SmtpClient();

            try
            {
                Console.WriteLine($"Connecting to SMTP server {_config["EmailSettings:SmtpHost"]}...");
                await smtp.ConnectAsync(
                    _config["EmailSettings:SmtpHost"],
                    int.Parse(_config["EmailSettings:SmtpPort"]),
                    SecureSocketOptions.StartTls);

                Console.WriteLine("Authenticating SMTP...");
                await smtp.AuthenticateAsync(
                    _config["EmailSettings:SmtpUsername"],
                    _config["EmailSettings:SmtpPassword"]);

                Console.WriteLine($"Sending email to {toEmail}...");
                await smtp.SendAsync(email);

                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email send failed: {ex.Message}");
                // Optional: rethrow or handle differently if needed
            }
            finally
            {
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                    Console.WriteLine("Disconnected from SMTP server.");
                }
            }
        }
    }
}

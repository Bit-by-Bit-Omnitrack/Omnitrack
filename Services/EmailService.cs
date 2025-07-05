using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace UserRoles.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config; // Access to appsettings.json values
        private readonly ILogger<EmailService> _logger; // For logging info, warnings, errors

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {   
            // Create the MIME email message
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:FromEmail"])); // Sender
            email.To.Add(MailboxAddress.Parse(toEmail)); // Recipient
            email.Subject = subject; // Subject
            email.Body = new TextPart(MimeKit.Text.TextFormat.Text) // Plain text body
            {
                Text = message
            };

            // Use MailKit's SMTP client
            using var smtp = new SmtpClient();

            try
            {
                // Connect to the SMTP server using STARTTLS for encryption
                _logger.LogInformation($"Connecting to SMTP server {_config["EmailSettings:SmtpHost"]}...");
                await smtp.ConnectAsync(
                    _config["EmailSettings:SmtpHost"],
                    int.Parse(_config["EmailSettings:SmtpPort"]),
                    SecureSocketOptions.StartTls);

                // Authenticate using Brevo login credentials
                _logger.LogInformation("Authenticating SMTP...");
                await smtp.AuthenticateAsync(
                    _config["EmailSettings:SmtpUsername"],
                    _config["EmailSettings:SmtpPassword"]);

                // Send the email
                _logger.LogInformation($"Sending email to {toEmail}...");
                await smtp.SendAsync(email);

                _logger.LogInformation("Email sent successfully.");
            }
            catch (Exception ex)
            {
                // Log any errors that occur during the send process
                _logger.LogError(ex, "Email send failed for {Recipient}", toEmail);
                
            }
            finally
            {
                // Ensure the connection is closed cleanly
                if (smtp.IsConnected)
                {
                    await smtp.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server.");
                }
            }
        }
    }
}

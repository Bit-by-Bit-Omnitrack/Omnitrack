using Microsoft.AspNetCore.Identity; // Add this using directive
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace Omnitrack.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement the email sending logic here
            return Task.CompletedTask;
        }
    }
}

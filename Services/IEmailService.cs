using System.Threading.Tasks;

namespace UserRoles.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
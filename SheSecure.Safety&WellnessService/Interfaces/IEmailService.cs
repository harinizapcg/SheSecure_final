using System.Threading.Tasks;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}

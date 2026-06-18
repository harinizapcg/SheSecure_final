using MailKit.Net.Smtp;
using MimeKit;

namespace SheSecure.NotificationService.Services
{
    public class EmailService
    {
        private const string SenderEmail = "abc566219@gmail.com";
        private const string AppPassword = "zgug vezv xpfk cyqr";

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress("SheSecure", SenderEmail));

            email.To.Add(
                MailboxAddress.Parse(toEmail));

            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                "smtp.gmail.com",
                587,
                MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                SenderEmail,
                AppPassword);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
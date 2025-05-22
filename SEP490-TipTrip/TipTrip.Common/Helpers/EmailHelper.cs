using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;

namespace FOCS.Common.Helpers
{
    public class EmailHelper : IEmailHelper
    {
        private readonly EmailModels _emailSettings;

        public EmailHelper(IOptions<EmailModels> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}

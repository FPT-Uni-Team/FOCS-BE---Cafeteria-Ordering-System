using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using TipTrip.Common.Interfaces;
using TipTrip.Common.Models;

namespace TipTrip.Common.Helpers
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
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.Host, _emailSettings.Port, false);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}

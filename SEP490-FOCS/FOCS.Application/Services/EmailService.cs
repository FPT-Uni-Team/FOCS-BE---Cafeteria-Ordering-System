using System;
using System.Threading.Tasks;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FOCS.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailHelper _emailHelper;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(IEmailHelper emailHelper, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _emailHelper = emailHelper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string accToken)
        {
            var baseApiUrl = _configuration["applicationProductUrl:BaseApiUrl"] ?? "http://localhost:5257";
            var confirmationLink = $"{baseApiUrl}/api/me/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(accToken)}";
            var subject = "Confirm your TipTrip account";
            var body = $@"
                <p>Hello,</p>
                <p>Thank you for registering. Please confirm your email by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>If you did not request this, please ignore this email.</p>";

            _logger.LogInformation("Sending email confirmation to {Email}", email);
            await _emailHelper.SendEmailAsync(email, subject, body);

            return true;
        }

        public async Task<bool> SendPasswordResetAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var subject = "TipTrip - Your New Password";
            var body = $@"
                <p>Hello,</p>
                <p>Your password has been reset. Here is your new password:</p>
                <p><strong>{resetPasswordRequest.NewPassword}</strong></p>
                <p>For security, please log in and change your password immediately.</p>";

            _logger.LogInformation("Sending reset password to {Email}", resetPasswordRequest.Email);
            await _emailHelper.SendEmailAsync(resetPasswordRequest.Email, subject, body);

            return true;
        }

        public async Task<bool> SendPasswordResetLinkAsync(string email, string resetToken)
        {
            var baseWebUrl = _configuration["applicationProductUrl:BaseWebUrl"] ?? "http://localhost:5257";
            var callbackUrl = $"{baseWebUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
            var subject = "TipTrip - Reset Your Password";
            var body = $@"
                <p>Hello,</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{callbackUrl}'>Reset Password</a></p>
                <p>If you did not request a password reset, please ignore this email.</p>";

            _logger.LogInformation("Sending email reset password to {Email}", email);
            await _emailHelper.SendEmailAsync(email, subject, body);

            return true;
        }
    }
}

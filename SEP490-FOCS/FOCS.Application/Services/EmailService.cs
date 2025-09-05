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

        public async Task<bool> SendEmailConfirmationAsync(string email, string accToken, string storeId, string tableId)
        {
            var baseApiUrl = _configuration["applicationProductUrl:BaseApiUrl"] ?? "http://localhost:5257";
            var confirmationLink = $"{baseApiUrl}/api/me/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(accToken)}&storeId={Uri.EscapeDataString(storeId)}&tableId={Uri.EscapeDataString(tableId)}";
            var subject = "Confirm Your FOCS Account";

            var body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #fafafa;'>
                            <h2 style='color: #333; text-align: center;'>Welcome to FOCS!</h2>
                            <p style='font-size: 16px; color: #333;'>Hi there,</p>
                            <p style='font-size: 16px; color: #333;'>
                                Thank you for registering with <strong>FOCS</strong>. Please confirm your email address by clicking the button below:
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{confirmationLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; font-size: 16px; border-radius: 5px; display: inline-block;'>Confirm Email</a>
                            </div>
                            <p style='font-size: 14px; color: #666;'>
                                If you didn’t request this, you can safely ignore this email.
                            </p>
                            <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;' />
                            <p style='font-size: 12px; color: #999; text-align: center;'>
                                &copy; {DateTime.UtcNow.Year} FOCS. All rights reserved.
                            </p>
                        </div>";

            _logger.LogInformation("Sending email confirmation to {Email}", email);
            await _emailHelper.SendEmailAsync(email, subject, body);

            return true;
        }


        public async Task<bool> SendPasswordResetAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var subject = "FOCS - Your New Password";
            var body = $@"
                <p>Hello,</p>
                <p>Your password has been reset. Here is your new password:</p>
                <p><strong>{resetPasswordRequest.NewPassword}</strong></p>
                <p>For security, please log in and change your password immediately.</p>";

            _logger.LogInformation("Sending reset password to {Email}", resetPasswordRequest.Email);
            await _emailHelper.SendEmailAsync(resetPasswordRequest.Email, subject, body);

            return true;
        }

        public async Task<bool> SendPasswordResetLinkAsync(string email, string resetToken, string storeId, string tableId)
        {
            var baseWebUrl = _configuration["applicationProductUrl:BaseStoreFrontUrl"] ?? "http://localhost:5257";
            var callbackUrl = $"{baseWebUrl}/{storeId}/{tableId}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
            var subject = "FOCS - Reset Your Password";
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using System.Security.Claims;
using MimeKit.Cryptography;
using FOCS.Common.Exceptions;
using FOCS.Common.Utils;
using FOCS.Order.Infrastucture.Entities;

namespace FOCS.Controllers
{
    [Route("api/me")]
    [ApiController]
    public class AuthController : FocsController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<AuthResult> LoginAsync(LoginRequest loginRequest)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(StoreId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat);
            return await _authService.LoginAsync(loginRequest, storeIdGuid);
        }

        [HttpPost("register")]
        public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(StoreId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat);
            return await _authService.RegisterAsync(registerRequest, storeIdGuid);
        }

        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.LogoutAsync(UserId);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.ForgotPasswordAsync(request.Email);

            if (!result)
                return NotFound(new { message = "User not found with the provided email." });

            return Ok(new { message = "A new reset password link has been sent to your email." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result)
                return BadRequest("Reset password failed. The token might be invalid or expired.");

            return Ok("Password has been reset successfully.");
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var result = await _authService.ConfirmEmailAsync(email, token);
            if (result)
            {
                return Ok(new { message = "Email confirmed successfully!" });
            }
            else
            {
                return BadRequest(new { message = "Email confirmation failed." });
            }
        }

        [HttpPost("change-password")]
        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            return await _authService.ChangePassword(request, Email);
        }

        [HttpPost("refresh-token")]
        public async Task<AuthResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            ConditionCheck.CheckCondition(refreshToken != null, Errors.Common.NotFound, "refresh token");

            ConditionCheck.CheckCondition(Guid.TryParse(StoreId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat);
            return await _authService.RefreshTokenAsync(refreshToken, storeIdGuid);
        }
    }
}

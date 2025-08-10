using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FOCS.Controllers
{
    [Route("api/me")]
    [ApiController]
    public class AuthController : FocsController
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly OtpService _smsService;

        public AuthController(IAuthService authService, IConfiguration configuration, OtpService smsService)
        {
            _authService = authService;
            _smsService = smsService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequest loginRequest)
        {
            if(!Guid.TryParse(StoreId, out Guid tryParseGuid))
            {
                return BadRequest();
            }

            return Ok(await _authService.LoginAsync(loginRequest, tryParseGuid));
        }

        [HttpPost("register")]
        public async Task<bool> RegisterAsUserAsync(RegisterRequest registerRequest)
        {
            ConditionCheck.CheckCondition(Guid.TryParse(StoreId, out Guid storeIdGuid), Errors.Common.InvalidGuidFormat);
            return await _authService.RegisterAsync(registerRequest, storeIdGuid, Roles.User);
        }

        [HttpPost("admin/register")]
        public async Task<bool> RegisterAsAdminAsync(RegisterRequest registerRequest)
        {
            return await _authService.RegisterAsync(registerRequest, Guid.Empty, Roles.Admin);
        }

        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.LogoutAsync(UserId);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromQuery] string phone)
        {
            await _smsService.SendOtpAsync(phone);
            return Ok();
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromQuery] string phone, [FromQuery] string otp)
        {
            await _smsService.VerifyOtpAsync(phone, otp);
            return Ok();
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
                return Redirect(_configuration["applicationProductUrl:BaseStoreFrontUrl"] + "/vi/sign-in");
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

        [HttpPost("mobile-token")]
        public async Task<bool> AddOrUpdateMobileToken(MobileTokenRequest request)
        {
            return await _authService.CreateOrUpdateMobileToken(request);
        }
    }
}

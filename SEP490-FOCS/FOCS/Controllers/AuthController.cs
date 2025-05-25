using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Formats.Asn1;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using System.Security.Claims;

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
            return await _authService.LoginAsync(loginRequest);
        }

        [HttpPost("register")]
        public async Task<AuthResult> RegisterAsync(RegisterRequest registerRequest)
        {
            return await _authService.RegisterAsync(registerRequest);
        }

        [HttpPost("logout")]
        public async Task Logout()
        {
            await _authService.LogoutAsync(UserId);
        }


        [HttpPost("change-password")]
        public async Task<bool> ChangePassword(ChangePasswordRequest request, string email)
        {
            return await _authService.ChangePassword(request, Email);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TipTrip.Common.Models;

namespace TipTrip.Common.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task LogoutAsync(string userId);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    }
}

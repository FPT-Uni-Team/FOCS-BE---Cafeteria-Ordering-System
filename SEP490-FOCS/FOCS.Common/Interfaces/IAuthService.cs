using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Models;

namespace FOCS.Common.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request, Guid StoreId);
        Task<AuthResult> LoginAsync(LoginRequest request, Guid StoreId);
        Task<AuthResult> RefreshTokenAsync(string refreshToken, Guid storeId);
        Task LogoutAsync(string userId);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ChangePassword(ChangePasswordRequest request, string email);

        Task<bool> CreateOrUpdateMobileToken(MobileTokenRequest request);
    }
}

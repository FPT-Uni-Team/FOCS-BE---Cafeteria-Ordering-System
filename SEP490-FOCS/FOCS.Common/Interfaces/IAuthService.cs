using FOCS.Common.Models;

namespace FOCS.Common.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequest request, Guid StoreId, string roles);
        Task<AuthResult> LoginAsync(LoginRequest request, string? StoreId = null);
        Task<AuthResult> RefreshTokenAsync(string refreshToken, Guid storeId);
        Task LogoutAsync(string userId);
        Task<bool> ConfirmEmailAsync(string email, string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
        Task<bool> ChangePassword(ChangePasswordRequest request, string email);

        Task<bool> CreateOrUpdateMobileToken(MobileTokenRequest request);
    }
}

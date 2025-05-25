using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FOCS.Common.Constants;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using Microsoft.Extensions.Logging;
using FOCS.Common.Utils;

namespace FOCS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ITokenService _tokenService;
        private readonly IRepository<UserRefreshToken> _userRefreshTokenRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration config, IMapper mapper, IEmailService emailService, ITokenService tokenService,
            IRepository<UserRefreshToken> userRepo, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = config;
            _mapper = mapper;
            _emailService = emailService;
            _tokenService = tokenService;
            _userRefreshTokenRepository = userRepo;
            _logger = logger;
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Generate a new password
            var newPassword = "NewPwd@" + Guid.NewGuid().ToString("N")[..6] ; // ví dụ: NewPwdA1B2C3

            // create a token for password reset
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // reset the password using the token
            var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!result.Succeeded) return false;

            // send email with new password
            var resetRequest = new ResetPasswordRequest
            {
                Email = email,
                Token = resetToken,
                NewPassword = newPassword
            };
            await _emailService.SendPasswordResetAsync(resetRequest);

            // Lock the user account for a short period to prevent abuse
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(1);
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string>() { Errors.Common.NotFound }
                };
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string>() { Errors.AuthError.WrongPassword }
                };
            }

            if (!user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                await _emailService.SendEmailConfirmationAsync(user.Email, token);

                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string>() { Errors.AuthError.NotVerifyAccount }
                };
            }

            return await GenerateAuthResult(user);

        }

        public async Task LogoutAsync(string userId)
        {
            var tokens = await _userRefreshTokenRepository.FindAsync(t => t.UserId == userId && !t.IsRevoked);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            await _userRefreshTokenRepository.SaveChangesAsync();
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = (await _userRefreshTokenRepository.FindAsync(x => x.Token == refreshToken && !x.IsRevoked)).FirstOrDefault();

            if (tokenEntity == null || tokenEntity.ExpirationDate < DateTime.Now)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { Errors.AuthError.InvalidRefreshToken }
                };
            }

            var user = await _userManager.FindByIdAsync(tokenEntity.UserId);

            if (user == null)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string> { Errors.Common.NotFound }
                };
            }

            tokenEntity.IsRevoked = true;
            await _userRefreshTokenRepository.SaveChangesAsync();

            return await GenerateAuthResult(user);
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return new AuthResult { IsSuccess = false, Errors = new[] { Errors.AuthError.PasswordNotMatch } };
            }

            var user = new User
            {
                Email = request.Email,
                FirstName = request.Firstname,
                LastName = request.Lastname,
                UserName = request.Email,
                PhoneNumber = request.Phone
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResult { IsSuccess = false, Errors = result.Errors.Select(e => e.Description) };
            }

            await _userManager.AddToRoleAsync(user, Roles.User);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send confirmation email (external email service assumed)
            await _emailService.SendEmailConfirmationAsync(user.Email, token);

            return await GenerateAuthResult(user);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null) return false;

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            return result.Succeeded;
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                ConditionCheck.CheckCondition(user != null, Errors.Common.NotFound);

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                ConditionCheck.CheckCondition(changePasswordResult.Succeeded, 
                    string.Join("; ", changePasswordResult.Errors.Select(e => e.Description)));

                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation($"User changed their password successfully.");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #region private method
        private async Task<AuthResult> GenerateAuthResult(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var jwt = tokenHandler.WriteToken(token);

            //save refresh token to db
            var userRefreshTokenDTO = new UserRefreshTokenDTO
            {
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                RefreshToken = refreshToken,
                UserId = user.Id
            };

            await _userRefreshTokenRepository.AddAsync(_mapper.Map<UserRefreshToken>(userRefreshTokenDTO));
            await _userRefreshTokenRepository.SaveChangesAsync();

            return new AuthResult
            {
                IsSuccess = true,
                AccessToken = jwt,
                RefreshToken = refreshToken,
                Errors = null
            };
        }
        #endregion
    }
}

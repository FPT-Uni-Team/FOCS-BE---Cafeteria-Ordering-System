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
using Newtonsoft.Json.Linq;
using FOCS.Order.Infrastucture.Entities;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

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
        private readonly IRepository<Store> _storeRepository;
        private readonly ILogger<AuthService> _logger;

        private readonly IRepository<MobileTokenDevice> _mobileTokenDevice;

        private readonly IRepository<UserStore> _userStoreRepository;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
            IConfiguration config, IMapper mapper, IEmailService emailService, ITokenService tokenService,
            IRepository<UserRefreshToken> userRepo, IRepository<Store> storeRepository, ILogger<AuthService> logger, IRepository<UserStore> userStoreRepository, IRepository<MobileTokenDevice> mobileTokenDevice)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = config;
            _mapper = mapper;
            _emailService = emailService;
            _tokenService = tokenService;
            _userRefreshTokenRepository = userRepo;
            _storeRepository = storeRepository;
            _logger = logger;
            _userStoreRepository = userStoreRepository;
            _mobileTokenDevice = mobileTokenDevice;
        }

        public async Task<bool> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);
            // generate reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            return await _emailService.SendPasswordResetLinkAsync(email, resetToken);
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request, Guid storeId)
        {
            var store = await _storeRepository.GetByIdAsync(storeId);
            if (store == null)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Errors = new List<string>() { Errors.Common.StoreNotFound }
                };
            }

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

            var userStores = await _userStoreRepository.FindAsync(x => x.UserId == Guid.Parse(user.Id));
            if(!userStores.Any(x => x.UserId == Guid.Parse(user.Id) && x.StoreId == storeId))
            {
                var newUserStore = new UserStoreDTO
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(user.Id),
                    StoreId = storeId,
                    BlockReason = null,
                    JoinDate = DateTime.UtcNow,
                    Status = Common.Enums.UserStoreStatus.Active
                };
                try
                {
                    await _userStoreRepository.AddAsync(_mapper.Map<UserStore>(newUserStore));
                    await _userStoreRepository.SaveChangesAsync();
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return await GenerateAuthResult(user, storeId);

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

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken, Guid storeId)
        {
            var tokenEntity = (await _userRefreshTokenRepository.FindAsync(x => x.Token == refreshToken && !x.IsRevoked)).FirstOrDefault();

            if (tokenEntity == null || tokenEntity.ExpirationDate < DateTime.UtcNow)
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

            return await GenerateAuthResult(user, storeId);
        }

        public async Task<bool> RegisterAsync(RegisterRequest request, Guid StoreId)
        {
            var store = await _storeRepository.GetByIdAsync(StoreId);
            ConditionCheck.CheckCondition(store != null, Errors.Common.StoreNotFound);

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Email,
                PhoneNumber = request.Phone
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            ConditionCheck.CheckCondition(result.Succeeded,
                    string.Join("; ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, Roles.User);

            var newUserStore = new UserStoreDTO
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(user.Id),
                StoreId = StoreId,
                BlockReason = null,
                JoinDate = DateTime.UtcNow,
                Status = Common.Enums.UserStoreStatus.Active
            };

            await _userStoreRepository.AddAsync(_mapper.Map<UserStore>(newUserStore));
            await _userStoreRepository.SaveChangesAsync();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send confirmation email (external email service assumed)
            await _emailService.SendEmailConfirmationAsync(user.Email, token);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            return result.Succeeded;
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request, string email)
        {
            ConditionCheck.CheckCondition(request.OldPassword != request.NewPassword, Errors.AuthError.PasswordReuse, Errors.FieldName.NewPassword);

            var user = await _userManager.FindByEmailAsync(email);
            ConditionCheck.CheckCondition(user != null, Errors.Common.UserNotFound);

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            ConditionCheck.CheckCondition(changePasswordResult.Succeeded,
                string.Join("; ", changePasswordResult.Errors.Select(e => e.Description)));

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation($"User changed their password successfully.");

            return true;
        }

        public async Task<bool> CreateOrUpdateMobileToken(MobileTokenRequest request)
        {
            try
            {
                var existing = await _mobileTokenDevice.AsQueryable().FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId);

                if (existing == null)
                {
                    await _mobileTokenDevice.AddAsync(new MobileTokenDevice
                    {
                        Id = Guid.NewGuid(),
                        DeviceId = request.DeviceId,
                        CreatedAt = DateTime.UtcNow,
                        LastUsedAt = DateTime.UtcNow,
                        Platform = request.Platform,
                        Token = request.Token,
                        UserId = request.ActorId
                    });

                }
                else
                {
                    existing.Token = request.Token;
                    existing.LastUsedAt = request.LastUsedAt;

                    _mobileTokenDevice.Update(existing);
                }

                await _mobileTokenDevice.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        #region private method
        private async Task<AuthResult> GenerateAuthResult(User user, Guid storeId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>();

            claims.AddRange(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("StoreId", storeId.ToString())
            }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role))));

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

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
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Errors = null
            };
        }
        #endregion
    }
}

using FOCS.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class OtpService
    {
        private readonly SmsService _esmsService;
        private readonly IRedisCacheService _redisService;

        public OtpService(SmsService esmsService, IRedisCacheService redisService)
        {
            _esmsService = esmsService;
            _redisService = redisService;
        }

        public async Task<object> SendOtpAsync(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            await _redisService.SetAsync($"OTP:{phoneNumber}", otp, TimeSpan.FromMinutes(5));
            return await _esmsService.SendSmsAsync(phoneNumber, $"Mã OTP của bạn là: {otp}");
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
        {
            var cachedOtp = await _redisService.GetAsync<string>($"OTP:{phoneNumber}");
            if (cachedOtp != null && cachedOtp == otp)
            {
                await _redisService.RemoveAsync($"OTP:{phoneNumber}");
                return true;
            }
            return false;
        }
    }
}

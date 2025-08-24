using FOCS.Common.Models;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace FOCS.Application.Services
{
    public class SmsService
    {
        private readonly TwilioSettings _settings;

        public SmsService(IOptions<TwilioSettings> settings)
        {
            _settings = settings.Value;

            // Khởi tạo Twilio client
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }

        public async Task<string> SendSmsAsync(string toPhoneNumber, string message)
        {
            try
            {
                var testPhone = "+84" + toPhoneNumber.Substring(1, 9);
                var messageResponse = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(_settings.FromPhoneNumber),
                    to: new Twilio.Types.PhoneNumber("+84" + toPhoneNumber.Substring(1, 9))
                );

                return $"SID: {messageResponse.Sid}, Status: {messageResponse.Status}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

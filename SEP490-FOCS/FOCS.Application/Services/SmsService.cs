using FOCS.Common.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace FOCS.Application.Services
{
    public class SmsService
    {
        private readonly EsmsSettings _settings;
        private readonly HttpClient _httpClient;

        public SmsService(IOptions<EsmsSettings> settings)
        {
            _settings = settings.Value;
            _httpClient = new HttpClient();
        }

        public async Task<string> SendSmsAsync(string phoneNumber, string message)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["ApiKey"] = _settings.ApiKey;
            query["SecretKey"] = _settings.SecretKey;
            query["Phone"] = phoneNumber;
            query["Content"] = message;
            query["Brandname"] = _settings.BrandName;
            query["SmsType"] = _settings.SmsType;
            query["IsUnicode"] = _settings.IsUnicode;

            var url = $"https://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?{query}";

            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
    }
}

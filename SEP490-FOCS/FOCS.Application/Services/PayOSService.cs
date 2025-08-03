using FOCS.Common.Interfaces;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;

        public PayOSService(string clientId, string apiKey, string checkSumKey)
        {
            _payOS = new PayOS(clientId, apiKey, checkSumKey);
        }

        public async Task<string> CreatePaymentLink(int amount, long orderCode, string description, List<ItemData> items, string returnUrl, string cancelUrl, string buyerName, string buyerPhone)
        {
            var rqBody = new PaymentData(orderCode, amount, description, items, cancelUrl, returnUrl, buyerName: buyerName, buyerPhone: buyerPhone);

            var response = await _payOS.createPaymentLink(rqBody);

            return response.checkoutUrl;
        }

        public async Task<PaymentLinkInformation> getPaymentLinkInformation(long linkId)
        {
            return await _payOS.getPaymentLinkInformation(linkId);
        }

        public async Task<PaymentLinkInformation> cancelPaymentLink(long linkId, string? cancelReason = null)
        {
            return await _payOS.cancelPaymentLink(linkId, cancelReason);
        }

        public async Task<string> confirmWebhook(string url)
        {
            return await _payOS.confirmWebhook(url);
        }

        public string VerifyWebhook(WebhookType hookType)
        {
            return _payOS.verifyPaymentWebhookData(hookType).code;
        }
    }
}

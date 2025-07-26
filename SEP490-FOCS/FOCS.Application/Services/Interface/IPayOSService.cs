using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IPayOSService
    {
        string VerifyWebhook(WebhookType hookType);

        Task<string> CreatePaymentLink(int amount, long orderCode, string description, List<ItemData> items, string returnUrl, string cancelUrl, string buyerName, string buyerPhone);

        Task<PaymentLinkInformation> cancelPaymentLink(long linkId, string? cancelReason = null);
    }
}

using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace FOCS.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : FocsController
    {
        private readonly IPayOSServiceFactory _payOSServiceFactory;
        private readonly IOrderService _orderService;
        private readonly IAdminStoreService _storeService;

        public PaymentController(IPayOSServiceFactory payOSServiceFactory, IOrderService orderService, IAdminStoreService storeService)
        {
            _payOSServiceFactory = payOSServiceFactory;
            _orderService = orderService;
            _storeService = storeService;
        }

        [HttpPost("update-config")]
        public async Task<bool> UpdateConfig(UpdateConfigPaymentRequest request)
        {
            return await _storeService.UpdateConfigPayment(request, StoreId);
        }

        [HttpPost("create-bank")]
        public async Task<bool> CreatePayment(CreatePaymentRequest request)
        {
            return await _storeService.CreatePaymentAsync(request, StoreId);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePaymentLink(GeneratePaymentLinkRequest request)
        {
            var order = await _orderService.GetOrderByCodeAsync(request.OrderCode);
            if (order == null)
                return NotFound("Order not found");

            var store = await _storeService.GetStoreSetting(order.StoreId);
            if (store == null)
                return NotFound("Store setting not found");

            var payOSService = _payOSServiceFactory.Create(store.PayOSClientId!, store.PayOSApiKey!, store.PayOSChecksumKey!);

            var paymentLink = await payOSService.CreatePaymentLink(request.Amount, request.OrderCode, request.Description, null, returnUrl: "https://focs.site/api/payment/webhook",cancelUrl: "https://focs.site/api/payment/cancel", buyerName: UserId, buyerPhone: "09123912763");
            
            return string.IsNullOrEmpty(paymentLink) ? BadRequest() : Ok(paymentLink);
        }

        //[HttpPost("cancel")]
        //public async Task<IActionResult> CancelPayment(CreatePaymentRequest request)
        //{
        //    var store = await _storeService.GetStoreSetting(Guid.Parse(StoreId));
        //    if (store == null)
        //        return NotFound("Store setting not found");

        //    var payOSService = _payOSServiceFactory.Create(store.PayOSClientId!, store.PayOSApiKey!, store.PayOSChecksumKey!);

        //    var payOS = await payOSService.cancelPaymentLink();
        //}


        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] WebhookType webhookData)
        {
            if (!webhookData.success)
                return Unauthorized();

            var order = await _orderService.GetOrderByCodeAsync(webhookData.data.orderCode);
            if (order == null)
                return NotFound("Order not found");

            var store = await _storeService.GetStoreSetting(order.StoreId);
            if (store == null)
                return NotFound("Store setting not found");

            var payOSService = _payOSServiceFactory.Create(store.PayOSClientId!, store.PayOSApiKey!, store.PayOSChecksumKey!);

            var verifiedCode = payOSService.VerifyWebhook(webhookData);
            if (verifiedCode != "00") 
                return Unauthorized();

            await _orderService.MarkAsPaid(webhookData.data.orderCode);

            return Ok();
        }

    }
}

using FOCS.Application.Services.Interface; 
using FOCS.Common.Interfaces;             
using FOCS.Common.Models;                 
using FOCS.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace FOCS.UnitTest.PaymentServiceTest
{
    public class GeneratePaymentLinkTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<IAdminStoreService> _storeServiceMock;
        private readonly Mock<IPayOSServiceFactory> _factoryMock;
        private readonly Mock<IPayOSService> _payOsServiceMock;
        private readonly PaymentController _controller;
        private readonly Guid _storeId = Guid.NewGuid();

        public GeneratePaymentLinkTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _storeServiceMock = new Mock<IAdminStoreService>();
            _factoryMock = new Mock<IPayOSServiceFactory>();
            _payOsServiceMock = new Mock<IPayOSService>();

            // Factory trả về mock IPayOSService
            _factoryMock
                .Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_payOsServiceMock.Object);

            _controller = new PaymentController(
                _factoryMock.Object,
                _orderServiceMock.Object,
                _storeServiceMock.Object
            );

            // Thiết HttpContext.User để controller.UserId = "test-user"
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private GeneratePaymentLinkRequest NewRequest(
            long code = 100, int amount = 500, string desc = "d")
            => new GeneratePaymentLinkRequest
            {
                OrderCode = code,
                Amount = amount,
                Description = desc,
                Items = null
            };

        private OrderDTO NewOrderDto() => new OrderDTO
        {
            Id = Guid.NewGuid(),
            OrderCode = "100",
            StoreId = _storeId,
            // các trường còn lại không quan trọng cho test
        };

        private StoreAdminResponse NewSetting() => new StoreAdminResponse
        {
            PayOSClientId = "cid",
            PayOSApiKey = "key",
            PayOSChecksumKey = "chk"
        };

        [Fact]
        public async Task OrderNotFound_ReturnsNotFound()
        {
            _orderServiceMock
                .Setup(s => s.GetOrderByCodeAsync(It.IsAny<long>()))
                .ReturnsAsync((OrderDTO?)null);

            var result = await _controller.GeneratePaymentLink(NewRequest());

            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Order not found", nf.Value);
        }

        [Fact]
        public async Task StoreSettingNotFound_ReturnsNotFound()
        {
            _orderServiceMock
                .Setup(s => s.GetOrderByCodeAsync(It.IsAny<long>()))
                .ReturnsAsync(NewOrderDto());

            _storeServiceMock
                .Setup(s => s.GetStoreSetting(_storeId))
                .ReturnsAsync((StoreAdminResponse?)null);

            var result = await _controller.GeneratePaymentLink(NewRequest());

            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Store setting not found", nf.Value);
        }

        [Fact]
        public async Task EmptyPaymentLink_ReturnsBadRequest()
        {
            _orderServiceMock
                .Setup(s => s.GetOrderByCodeAsync(It.IsAny<long>()))
                .ReturnsAsync(NewOrderDto());
            _storeServiceMock
                .Setup(s => s.GetStoreSetting(_storeId))
                .ReturnsAsync(NewSetting());

            _payOsServiceMock
                .Setup(p => p.CreatePaymentLink(
                    It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string?>(),
                    (List<Net.payOS.Types.ItemData>)It.IsAny<object?>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            var result = await _controller.GeneratePaymentLink(NewRequest());

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task ValidPaymentLink_ReturnsOkWithLink()
        {
            const string link = "https://pay.link/123";
            var req = NewRequest(code: 200, amount: 1500, desc: "order200");

            _orderServiceMock
                .Setup(s => s.GetOrderByCodeAsync(req.OrderCode))
                .ReturnsAsync(NewOrderDto());
            _storeServiceMock
                .Setup(s => s.GetStoreSetting(_storeId))
                .ReturnsAsync(NewSetting());
            _payOsServiceMock
                .Setup(p => p.CreatePaymentLink(
                    (int)req.Amount, req.OrderCode, req.Description,
                    null,
                    "https://focs.site/api/payment/webhook",
                    "https://focs.site/api/payment/cancel",
                    "test-user",
                    "09123912763"))
                .ReturnsAsync(link);

            var result = await _controller.GeneratePaymentLink(req);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(link, ok.Value);

            _factoryMock.Verify(f => f.Create("cid", "key", "chk"), Times.Once);
            _payOsServiceMock.Verify(p => p.CreatePaymentLink(
                (int)req.Amount, req.OrderCode, req.Description,
                null,
                "https://focs.site/api/payment/webhook",
                "https://focs.site/api/payment/cancel",
                "test-user",
                "09123912763"), Times.Once);
        }
    }
}

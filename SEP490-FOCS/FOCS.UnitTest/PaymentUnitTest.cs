using AutoMapper;
using FOCS.Application.Services;
using FOCS.Application.Services.Interface;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Models.Payment;
using FOCS.Controllers;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.DataProtection;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.UnitTest
{
    public class PaymentUnitTest
    {
        private readonly Mock<IRepository<Store>> _storeRepoMock = new();
        private readonly Mock<IRepository<StoreSetting>> _storeSettingRepoMock = new();
        private readonly Mock<IRepository<Brand>> _brandRepoMock = new();
        private readonly Mock<IRepository<PaymentAccount>> _paymentAccountRepoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<IDataProtectionProvider> _dataProtectorMock = new();

        private readonly AdminStoreService _adminStoreService;

        public PaymentUnitTest()
        {
            _adminStoreService = new AdminStoreService(
                _storeRepoMock.Object,
                _paymentAccountRepoMock.Object,
                _storeSettingRepoMock.Object,
                _mapperMock.Object,
                _dataProtectorMock.Object,
                _brandRepoMock.Object
            );
        }

        #region CreatePayment CM-70
        [Theory]
        [InlineData("Bank name string", "Bank code string", "0287319826472", "Account name", true)]
        [InlineData(null, "Bank code string", "0287319826472", "Account name", false)]
        [InlineData("Bank name string", null, "0287319826472", "Account name", false)]
        [InlineData("Bank name string", "Bank code string", null, "Account name", false)]
        [InlineData("Bank name string", "Bank code string", "0287319826472", null, false)]
        [InlineData(null, null, null, null, false)]
        public async Task CreatePayment_SimpleRun_ChecksIfServiceRuns(
            string bankName, string bankCode, string accountNumber, string accountName, bool shouldSucceed)
        {
            // Arrange
            var request = new CreatePaymentRequest
            {
                BankName = bankName,
                BankCode = bankCode,
                AccountNumber = accountNumber,
                AccountName = accountName
            };

            var storeId = Guid.NewGuid().ToString();

            // Setup mock
            _paymentAccountRepoMock.Setup(r => r.AsQueryable())
                .Returns(new List<PaymentAccount>().BuildMock());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _adminStoreService.CreatePaymentAsync(request, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion

        #region UpdateConfigPayment CM-71
        [Theory]
        [InlineData("Client Id", "Api Id", "Check sum key", true)]
        [InlineData(null, "Api Id", "Check sum key", false)]
        [InlineData("Client Id", null, "Check sum key", false)]
        [InlineData("Client Id", "Api Id", null, false)]
        [InlineData(null, null, null, false)]
        public async Task UpdateConfigPayment_SimpleRun_ChecksIfServiceRuns(
            string payOSClientId, string payOSApiKey, string payOSChecksumKey, bool shouldSucceed)
        {
            // Arrange
            var request = new UpdateConfigPaymentRequest
            {
                PayOSClientId = payOSClientId,
                PayOSApiKey = payOSApiKey,
                PayOSChecksumKey = payOSChecksumKey
            };

            var storeId = Guid.NewGuid().ToString();

            // Setup mock
            _storeSettingRepoMock.Setup(r => r.AsQueryable())
                .Returns(new List<StoreSetting>().BuildMock());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _adminStoreService.UpdateConfigPayment(request, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion

        #region GeneratePaymentLink CM-72
        [Theory]
        [InlineData(1234, 22, "Description string", null, true)]
        [InlineData(null, 22, "Description string", null, false)]
        [InlineData(1234, null, "Description string", null, false)]
        [InlineData(1234, 22, null, null, true)]
        [InlineData(null, null, null, null, false)]
        public async Task GeneratePaymentLink_SimpleRun_ChecksIfServiceRuns(
            long orderCode, double amount, string description, string items, bool shouldSucceed)
        {
            // Arrange
            var request = new GeneratePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = amount,
                Description = description,
                Items = items,
                TableId = Guid.NewGuid()
            };

            var mockOrderService = new Mock<IOrderService>();
            var mockStoreService = new Mock<IAdminStoreService>();
            var mockPayOSServiceFactory = new Mock<IPayOSServiceFactory>();

            var controller = new PaymentController(
                mockPayOSServiceFactory.Object,
                mockOrderService.Object,
                mockStoreService.Object
            );

            // Setup mocks
            mockOrderService.Setup(s => s.GetOrderByCodeAsync(orderCode))
                .ReturnsAsync(new OrderDTO { StoreId = Guid.NewGuid() });

            //mockStoreService.Setup(s => s.GetStoreSetting(It.IsAny<Guid>()))
            //    .ReturnsAsync(new StoreSettingDTO
            //    {
            //        PayOSClientId = "client123",
            //        PayOSApiKey = "apiKey456",
            //        PayOSChecksumKey = "checksum789"
            //    });

            mockPayOSServiceFactory.Setup(f => f.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Mock<IPayOSService>().Object);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await controller.GeneratePaymentLink(request);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion
    }
}

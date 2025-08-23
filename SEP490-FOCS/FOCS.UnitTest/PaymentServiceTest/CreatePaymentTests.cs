using FOCS.Common.Models.Payment;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.PaymentServiceTest
{
    public class CreatePaymentTests : PaymentServiceTestBase
    {
        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnTrueAndAddNewAccount_WhenNoExistingAccount()
        {
            // Arrange: không có account trùng
            SetupPaymentQueryable(new List<PaymentAccount>());

            var request = new CreatePaymentRequest
            {
                BankName = "BankA",
                BankCode = "001",
                AccountNumber = "123456",
                AccountName = "John Doe"
            };
            var storeId = Guid.NewGuid().ToString();

            // Act
            var result = await _adminStoreService.CreatePaymentAsync(request, storeId);

            // Assert kết quả
            Assert.True(result);
            _paymentAccountRepoMock.Verify(x => x.AddAsync(It.Is<PaymentAccount>(p =>
                p.BankName == request.BankName &&
                p.BankCode == request.BankCode &&
                p.AccountNumber == request.AccountNumber &&
                p.AccountName == request.AccountName &&
                p.StoreId == Guid.Parse(storeId) &&
                p.IsActive &&
                p.CreatedAt != null
            )), Times.Once);
            _paymentAccountRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnFalse_WhenAccountAlreadyExists()
        {
            // Arrange: đã có 1 account cùng BankName & AccountNumber
            var existing = new PaymentAccount
            {
                BankName = "BankA",
                AccountNumber = "123456"
            };
            SetupPaymentQueryable(new List<PaymentAccount> { existing });

            var request = new CreatePaymentRequest
            {
                BankName = "BankA",
                BankCode = "001",
                AccountNumber = "123456",
                AccountName = "John Doe"
            };
            var storeId = Guid.NewGuid().ToString();

            // Act
            var result = await _adminStoreService.CreatePaymentAsync(request, storeId);

            // Assert trả về false và không gọi Add/Save
            Assert.False(result);
            _paymentAccountRepoMock.Verify(x => x.AddAsync(It.IsAny<PaymentAccount>()), Times.Never);
            _paymentAccountRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnFalse_WhenSaveChangesThrows()
        {
            // Arrange: không có account trùng
            SetupPaymentQueryable(new List<PaymentAccount>());

            var request = new CreatePaymentRequest
            {
                BankName = "BankB",
                BankCode = "002",
                AccountNumber = "654321",
                AccountName = "Alice"
            };
            var storeId = Guid.NewGuid().ToString();

            // Mock cho AddAsync thành công, nhưng SaveChangesAsync ném exception
            _paymentAccountRepoMock
                .Setup(x => x.AddAsync(It.IsAny<PaymentAccount>()))
                .Returns(Task.CompletedTask);
            _paymentAccountRepoMock
                .Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB failure"));

            // Act
            var result = await _adminStoreService.CreatePaymentAsync(request, storeId);

            // Assert
            Assert.False(result);

            // AddAsync vẫn được gọi
            _paymentAccountRepoMock.Verify(x => x.AddAsync(It.IsAny<PaymentAccount>()), Times.Once);
            // SaveChangesAsync cũng được gọi và ném
            _paymentAccountRepoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentAsync_ShouldReturnFalse_WhenStoreIdIsInvalidGuid()
        {
            // Arrange: không có account trùng
            SetupPaymentQueryable(new List<PaymentAccount>());

            var request = new CreatePaymentRequest
            {
                BankName = "BankC",
                BankCode = "003",
                AccountNumber = "000000",
                AccountName = "Bob"
            };
            var badStoreId = "not-a-guid";

            // Act
            var result = await _adminStoreService.CreatePaymentAsync(request, badStoreId);

            // Assert: catch Guid.Parse exception → trả về false
            Assert.False(result);

            // Không gọi AddAsync hay SaveChangesAsync
            _paymentAccountRepoMock.Verify(x => x.AddAsync(It.IsAny<PaymentAccount>()), Times.Never);
            _paymentAccountRepoMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}

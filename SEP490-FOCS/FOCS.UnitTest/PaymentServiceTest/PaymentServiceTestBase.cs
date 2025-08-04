using AutoMapper;
using FOCS.Application.Services;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.AspNetCore.DataProtection;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;
using System.Text;

namespace FOCS.UnitTest.PaymentServiceTest
{
    public class PaymentServiceTestBase
    {
        // Repositories
        protected readonly Mock<IRepository<PaymentAccount>> _paymentAccountRepoMock;
        protected readonly Mock<IRepository<StoreSetting>> _storeSettingRepoMock;
        protected readonly Mock<IRepository<Store>> _storeRepoMock;

        // Other dependencies
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly FakeDataProtectionProvider _dpProvider;

        // Service under test
        protected readonly AdminStoreService _adminStoreService;

        public PaymentServiceTestBase()
        {
            _paymentAccountRepoMock = new Mock<IRepository<PaymentAccount>>();
            _storeSettingRepoMock = new Mock<IRepository<StoreSetting>>();
            _storeRepoMock = new Mock<IRepository<Store>>();

            _mapperMock = new Mock<IMapper>();

            _dpProvider = new FakeDataProtectionProvider();

            _adminStoreService = new AdminStoreService(
                _storeRepoMock.Object,
                _paymentAccountRepoMock.Object,
                _storeSettingRepoMock.Object,
                _mapperMock.Object,
                _dpProvider
            );
        }

        protected void SetupPaymentQueryable(List<PaymentAccount> list)
        {
            _paymentAccountRepoMock
                .Setup(r => r.AsQueryable())
                .Returns(list.AsQueryable().BuildMockDbSet().Object);
        }

        protected void SetupStoreSetting(StoreSetting setting)
        {
            _storeSettingRepoMock
                .Setup(r => r.AsQueryable())
                .Returns(new[] { setting }
                    .AsQueryable()
                    .BuildMockDbSet()
                    .Object);

            _storeSettingRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StoreSetting, bool>>>()))
                .ReturnsAsync(new List<StoreSetting> { setting });
        }

        protected void SetupStore(Guid storeId, Store store)
        {
            _storeRepoMock
                .Setup(r => r.GetByIdAsync(storeId))
                .ReturnsAsync(store);
        }

        protected IQueryable<PaymentAccount> BuildEmptyPaymentDbSet()
            => new List<PaymentAccount>().AsQueryable().BuildMockDbSet().Object;

        protected void AssertConditionException(Exception ex, string expectedMessage)
        {
            Assert.NotNull(ex);
            var parts = ex.Message.Split('@');
            Assert.Equal(expectedMessage, parts[0]);
        }
    }

    public class FakeDataProtector : IDataProtector
    {
        public IDataProtector CreateProtector(string purpose) => this;

        public byte[] Protect(byte[] plaintext)
        {
            var str = Encoding.UTF8.GetString(plaintext);
            var enc = "enc_" + str;
            return Encoding.UTF8.GetBytes(enc);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            var str = Encoding.UTF8.GetString(protectedData);
            if (str.StartsWith("enc_"))
                str = str.Substring(4);
            return Encoding.UTF8.GetBytes(str);
        }
    }

    public class FakeDataProtectionProvider : IDataProtectionProvider
    {
        private readonly IDataProtector _protector = new FakeDataProtector();
        public IDataProtector CreateProtector(string purpose) => _protector;
    }
}

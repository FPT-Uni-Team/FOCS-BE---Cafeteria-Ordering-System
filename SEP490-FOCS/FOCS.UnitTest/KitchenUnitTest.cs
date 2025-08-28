using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Enums;
using FOCS.Common.Interfaces;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MassTransit;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest
{
    public class KitchenUnitTest
    {
        private readonly Mock<IRepository<MenuItemVariant>> _menuItemVariantRepoMock = new();
        private readonly Mock<IRepository<OrderWrap>> _orderWrapRepoMock = new();
        private readonly Mock<IRepository<OrderEntity>> _orderRepoMock = new();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
        private readonly Mock<IRealtimeService> _realtimeServiceMock = new();
        private readonly Mock<IMobileTokenSevice> _mobileTokenServiceMock = new();
        private readonly Mock<IMapper> _mapperMock = new();

        private readonly OrderWrapService _orderWrapService;

        public KitchenUnitTest()
        {
            _orderWrapService = new OrderWrapService(
                _menuItemVariantRepoMock.Object,
                _orderWrapRepoMock.Object,
                _mapperMock.Object,
                _mobileTokenServiceMock.Object,
                _realtimeServiceMock.Object,
                _publishEndpointMock.Object,
                _orderRepoMock.Object
            );
        }

        #region GetListOrderWraps CM-50
        [Theory]
        [InlineData(1, 10, "name", "Thai", "created_date", "desc", true)]
        [InlineData(5, 10, "name", "Thai", "created_date", "desc", true)]
        [InlineData(null, 10, "name", "Thai", "created_date", "desc", false)]
        [InlineData(1, 20, "name", "Thai", "created_date", "desc", true)]
        [InlineData(1, null, "name", "Thai", "created_date", "desc", false)]
        [InlineData(1, 10, "staff", "Thai", "created_date", "desc", true)]
        [InlineData(1, 10, null, "Thai", "created_date", "desc", true)]
        [InlineData(1, 10, "name", null, "created_date", "desc", true)]
        [InlineData(1, 10, "name", "Thai", null, "desc", true)]
        [InlineData(1, 10, "name", "Thai", "created_date", null, true)]
        [InlineData(null, null, null, null, null, null, false)]
        public async Task GetListOrderWraps_SimpleRun_ChecksIfServiceRuns(
            int? page, int? pageSize, string searchBy, string searchValue, string sortBy, string sortOrder, bool shouldSuccess)
        {
            // Arrange
            var query = new UrlQueryParameters
            {
                Page = page ?? 1,
                PageSize = pageSize ?? 10,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Filters = new Dictionary<string, string>() 
            };

            var storeId = Guid.NewGuid().ToString();

            var orderWraps = new List<OrderWrap>
            {
                new OrderWrap
                {
                    Id = Guid.NewGuid(),
                    StoreId = Guid.Parse(storeId),
                    Code = "ORD001",
                    OrderWrapStatus = OrderWrapStatus.Created
                }
            }.AsQueryable();

            _orderWrapRepoMock.Setup(r => r.AsQueryable())
                .Returns(orderWraps.BuildMock());

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _orderWrapService.GetListOrderWraps(query, storeId);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion

        #region ChangeStatusProductionOrder CM-51
        [Theory]
        [InlineData("5e45861b-ac1d-4433-8bdc-ac48a18d8012", OrderWrapStatus.Created, true)]
        [InlineData("65a18f22-dd31-4ea6-b456-a775dbcfd62e", OrderWrapStatus.Created, false)]
        [InlineData(null, OrderWrapStatus.Created, false)]
        [InlineData("5e45861b-ac1d-4433-8bdc-ac48a18d8012", OrderWrapStatus.Finalized, true)]
        [InlineData("5e45861b-ac1d-4433-8bdc-ac48a18d8012", null, false)]
        [InlineData(null, null, false)]
        public async Task ChangeStatusProductionOrder_SimpleRun_ChecksIfServiceRuns(
            string orderWrapIdStr, OrderWrapStatus? status, bool shouldSuccess)
        {
            // Arrange
            Guid? orderWrapId = string.IsNullOrEmpty(orderWrapIdStr) ? (Guid?)null : Guid.Parse(orderWrapIdStr);

            var request = new UpdateStatusProductionOrderRequest
            {
                OrderWrapId = orderWrapId ?? Guid.Empty,
                Status = status ?? default
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await _orderWrapService.ChangeStatusProductionOrder(request);
            });

            // Assert
            Assert.Null(exception);
        }
        #endregion
    }
}

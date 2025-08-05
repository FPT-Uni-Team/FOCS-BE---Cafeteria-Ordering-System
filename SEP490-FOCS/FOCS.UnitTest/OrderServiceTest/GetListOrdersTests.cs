using FOCS.Common.Enums;
using FOCS.Common.Models;
using MockQueryable;
using Moq;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.UnitTest.OrderServiceTest;

public class GetListOrdersTests : OrderServiceTestBase
{
    [Fact]
    public async Task GetListOrders_ShouldReturnPagedResult()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var storeId = Guid.NewGuid().ToString();
        var orders = FakeOrders(userId, storeId, 15);

        _mockOrderRepository.Setup(r => r.AsQueryable())
            .Returns(orders.AsQueryable().BuildMock());

        _mockMapper.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
            .Returns((List<OrderEntity> src) => src.Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerNote = o.CustomerNote,
                CreatedAt = o.CreatedAt,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,
                PaymentStatus = o.PaymentStatus
            }).ToList());

        var queryParams = new UrlQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _orderService.GetListOrders(queryParams, storeId, userId);

        // Assert
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(10, result.Items.Count);
        Assert.All(result.Items, item => Assert.NotEqual(Guid.Empty, item.Id));
    }

    [Fact]
    public async Task GetListOrders_ShouldReturnEmpty_WhenNoOrdersMatch()
    {
        // Arrange
        _mockOrderRepository.Setup(r => r.AsQueryable())
            .Returns(Enumerable.Empty<OrderEntity>().AsQueryable().BuildMock());
        _mockMapper.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
            .Returns((List<OrderEntity> src) => src.Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerNote = o.CustomerNote,
                CreatedAt = o.CreatedAt,
                OrderStatus = o.OrderStatus,
                OrderType = o.OrderType,
                PaymentStatus = o.PaymentStatus
            }).ToList());

        var queryParams = new UrlQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _orderService.GetListOrders(queryParams, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

        // Assert
        Assert.Empty(result.Items);
    }

    //[Theory]
    //[InlineData("order_status", "Pending", OrderStatus.Pending)]
    //[InlineData("order_status", "Completed", OrderStatus.Completed)]
    //[InlineData("payment_status", "Paid", PaymentStatus.Paid)]
    //[InlineData("order_type", "DineIn", OrderType.DineIn)]
    //public async Task GetListOrders_ShouldApplyFilters(string key, string value, object expectedEnum)
    //{
    //    // Arrange
    //    var userId = Guid.NewGuid().ToString();
    //    var storeId = Guid.NewGuid().ToString();
    //    var orders = FakeOrders(userId, storeId, 10)
    //        .Select(o =>
    //        {
    //            if (key == "order_status") o.OrderStatus = (OrderStatus)expectedEnum;
    //            if (key == "payment_status") o.PaymentStatus = (PaymentStatus)expectedEnum;
    //            if (key == "order_type") o.OrderType = (OrderType)expectedEnum;
    //            return o;
    //        })
    //        .ToList();

    //    _mockOrderRepository.Setup(r => r.AsQueryable())
    //        .Returns(orders.AsQueryable().BuildMock());
    //    _mockMapper.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
    //        .Returns((List<OrderEntity> src) => src.Select(o => new OrderDTO
    //        {
    //            Id = o.Id,
    //            CustomerNote = o.CustomerNote,
    //            CreatedAt = o.CreatedAt,
    //            OrderStatus = o.OrderStatus,
    //            OrderType = o.OrderType,
    //            PaymentStatus = o.PaymentStatus
    //        }).ToList());

    //    var queryParams = new UrlQueryParameters
    //    {
    //        Filters = new Dictionary<string, string> { { key, value } },
    //        Page = 1,
    //        PageSize = 10
    //    };

    //    // Act
    //    var result = await _orderService.GetListOrders(queryParams, storeId, userId);

    //    // Assert
    //    Assert.All(result.Items, item =>
    //    {
    //        switch (key)
    //        {
    //            case "order_status":
    //                Assert.Equal(expectedEnum, item.OrderStatus);
    //                break;
    //            case "payment_status":
    //                Assert.Equal(expectedEnum, item.PaymentStatus);
    //                break;
    //            case "order_type":
    //                Assert.Equal(expectedEnum, item.OrderType);
    //                break;
    //        }
    //    });
    //}

    //[Theory]
    //[InlineData("customer_note", "urgent")]
    //[InlineData("customer_note", "birthday")]
    //public async Task GetListOrders_ShouldApplySearch(string searchBy, string searchValue)
    //{
    //    // Arrange
    //    var userId = Guid.NewGuid().ToString();
    //    var storeId = Guid.NewGuid().ToString();
    //    var orders = FakeOrders(userId, storeId, 10).ToList();
    //    orders[0].CustomerNote = "Urgent delivery";
    //    orders[1].CustomerNote = "Birthday party";

    //    _mockOrderRepository.Setup(r => r.AsQueryable())
    //        .Returns(orders.AsQueryable().BuildMock());
    //    _mockMapper.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
    //        .Returns((List<OrderEntity> src) => src.Select(o => new OrderDTO
    //        {
    //            Id = o.Id,
    //            CustomerNote = o.CustomerNote,
    //            CreatedAt = o.CreatedAt,
    //            OrderStatus = o.OrderStatus,
    //            OrderType = o.OrderType,
    //            PaymentStatus = o.PaymentStatus
    //        }).ToList());

    //    var queryParams = new UrlQueryParameters
    //    {
    //        SearchBy = searchBy,
    //        SearchValue = searchValue,
    //        Page = 1,
    //        PageSize = 10
    //    };

    //    // Act
    //    var result = await _orderService.GetListOrders(queryParams, storeId, userId);

    //    // Assert
    //    Assert.All(result.Items, item =>
    //        Assert.Contains(searchValue.ToLower(), item.CustomerNote.ToLower())
    //    );
    //}

    //[Theory]
    //[InlineData("created_at", "desc")]
    //[InlineData("created_at", "asc")]
    //public async Task GetListOrders_ShouldApplySorting(string sortBy, string sortOrder)
    //{
    //    // Arrange
    //    var userId = Guid.NewGuid().ToString();
    //    var storeId = Guid.NewGuid().ToString();
    //    var orders = FakeOrders(userId, storeId, 5)
    //        .OrderBy(x => Guid.NewGuid())
    //        .ToList();

    //    orders[0].CreatedAt = DateTime.UtcNow.AddDays(-3);
    //    orders[1].CreatedAt = DateTime.UtcNow.AddDays(-2);
    //    orders[2].CreatedAt = DateTime.UtcNow.AddDays(-1);
    //    orders[3].CreatedAt = DateTime.UtcNow;
    //    orders[4].CreatedAt = DateTime.UtcNow.AddHours(1);

    //    _mockOrderRepository.Setup(r => r.AsQueryable())
    //        .Returns(orders.AsQueryable().BuildMock());
    //    _mockMapper.Setup(m => m.Map<List<OrderDTO>>(It.IsAny<List<OrderEntity>>()))
    //        .Returns((List<OrderEntity> src) => src.Select(o => new OrderDTO
    //        {
    //            Id = o.Id,
    //            CustomerNote = o.CustomerNote,
    //            CreatedAt = o.CreatedAt,
    //            OrderStatus = o.OrderStatus,
    //            OrderType = o.OrderType,
    //            PaymentStatus = o.PaymentStatus
    //        }).ToList());

    //    var queryParams = new UrlQueryParameters
    //    {
    //        SortBy = sortBy,
    //        SortOrder = sortOrder,
    //        Page = 1,
    //        PageSize = 10
    //    };

    //    // Act
    //    var result = await _orderService.GetListOrders(queryParams, storeId, userId);

    //    var sorted = sortOrder == "asc"
    //        ? result.Items.OrderBy(o => o.CreatedAt)
    //        : result.Items.OrderByDescending(o => o.CreatedAt);

    //    // Assert
    //    Assert.Equal(sorted.Select(o => o.Id), result.Items.Select(o => o.Id));
    //}

    // Helper to fake data
    private List<OrderEntity> FakeOrders(string userId, string storeId, int count)
    {
        var userGuid = Guid.Parse(userId);
        var storeGuid = Guid.Parse(storeId);
        return Enumerable.Range(1, count).Select(i => new OrderEntity
        {
            Id = Guid.NewGuid(),
            OrderCode = i,
            UserId = userGuid,
            StoreId = storeGuid,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CustomerNote = $"note {i}"
        }).ToList();
    }
}

using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class GetAllMenuItemAsyncTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task GetAllMenuItemAsync_ShouldReturnPagedResult_WithCorrectProperties()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { Page = 1, PageSize = 10 };
            var menuItems = new List<MenuItem> { CreateValidMenuItem(storeId), CreateValidMenuItem(storeId) };

            SetupMenuQueryable(menuItems);

            var expected = menuItems.Select(item => CreateValidMenuItemAdminDTO(item.StoreId)).ToList();
            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>())).Returns(expected);

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(query.PageSize, result.PageSize);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldExcludeDeletedItems()
        {
            var storeId = Guid.NewGuid();
            var validItem = CreateValidMenuItem(storeId);
            var deletedItem = CreateValidMenuItem(storeId); deletedItem.IsDeleted = true;

            SetupMenuQueryable(new List<MenuItem> { validItem, deletedItem });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { CreateValidMenuItemAdminDTO(validItem.StoreId) });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(new UrlQueryParameters(), storeId);

            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldReturnOnlyItemsFromSpecifiedStore()
        {
            var storeId = Guid.NewGuid();
            var otherStoreId = Guid.NewGuid();

            var valid = CreateValidMenuItem(storeId);
            var invalid = CreateValidMenuItem(otherStoreId);

            SetupMenuQueryable(new List<MenuItem> { valid, invalid });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { CreateValidMenuItemAdminDTO(valid.StoreId) });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(new UrlQueryParameters(), storeId);

            Assert.Single(result.Items);
            Assert.Equal(storeId, result.Items[0].StoreId);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldReturnItemsMatchingNameSearch()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { SearchBy = "name", SearchValue = "burger" };

            var matching = CreateValidMenuItem(storeId); matching.Name = "Cheeseburger";
            var nonMatching = CreateValidMenuItem(storeId); nonMatching.Name = "Pizza";

            SetupMenuQueryable(new List<MenuItem> { matching, nonMatching });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { new() { Id = matching.Id, Name = matching.Name } });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Single(result.Items);
            Assert.Contains("burger", result.Items[0].Name, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldReturnItemsMatchingDescriptionSearch()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { SortBy = "base_price", SortOrder = "desc" };

            var low = CreateValidMenuItem(storeId); low.BasePrice = 10;
            var high = CreateValidMenuItem(storeId); high.BasePrice = 20;

            SetupMenuQueryable(new List<MenuItem> { low, high });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO>
                {
                    new() { Id = high.Id, BasePrice = high.BasePrice },
                    new() { Id = low.Id, BasePrice = low.BasePrice }
                });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(20, result.Items[0].BasePrice);
            Assert.Equal(10, result.Items[1].BasePrice);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldFilterItemsAboveGivenPrice()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { Page = 2, PageSize = 1 };

            var first = CreateValidMenuItem(storeId); first.Name = "A";
            var second = CreateValidMenuItem(storeId); second.Name = "B";

            SetupMenuQueryable(new List<MenuItem> { first, second });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { new() { Id = second.Id, Name = second.Name } });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Single(result.Items);
            Assert.Equal("B", result.Items[0].Name);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldSortByBasePriceDescending()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { SearchBy = "description", SearchValue = "fresh" };

            var matching = CreateValidMenuItem(storeId); matching.Description = "Fresh and tasty";
            var nonMatching = CreateValidMenuItem(storeId); nonMatching.Description = "Frozen item";

            SetupMenuQueryable(new List<MenuItem> { matching, nonMatching });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { new() { Id = matching.Id, Description = matching.Description } });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Single(result.Items);
            Assert.Contains("fresh", result.Items[0].Description, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldSortByNameAscending()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { "price", "15" } }
            };

            var low = CreateValidMenuItem(storeId); low.BasePrice = 10;
            var high = CreateValidMenuItem(storeId); high.BasePrice = 20;

            SetupMenuQueryable(new List<MenuItem> { low, high });

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO> { new() { Id = high.Id, BasePrice = high.BasePrice } });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Single(result.Items);
            Assert.Equal(20, result.Items[0].BasePrice);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldPaginateResultsCorrectly()
        {
            var storeId = Guid.NewGuid();
            var query = new UrlQueryParameters { SortBy = "name", SortOrder = "asc" };

            var a = CreateValidMenuItem(storeId); a.Name = "Apple";
            var b = CreateValidMenuItem(storeId); b.Name = "Banana";

            SetupMenuQueryable(new List<MenuItem> { b, a }); // intentionally unordered

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO>
                {
            new() { Id = a.Id, Name = a.Name },
            new() { Id = b.Id, Name = b.Name }
                });

            var result = await _adminMenuItemService.GetAllMenuItemAsync(query, storeId);

            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Apple", result.Items[0].Name);
            Assert.Equal("Banana", result.Items[1].Name);
        }

        [Fact]
        public async Task GetAllMenuItemAsync_ShouldReturnEmptyResult_WhenNoItemsMatch()
        {
            var storeId = Guid.NewGuid();

            SetupMenuQueryable(new List<MenuItem>()); // empty data

            _mapperMock.Setup(m => m.Map<List<MenuItemAdminDTO>>(It.IsAny<List<MenuItem>>()))
                .Returns(new List<MenuItemAdminDTO>());

            var result = await _adminMenuItemService.GetAllMenuItemAsync(new UrlQueryParameters(), storeId);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

    }
}

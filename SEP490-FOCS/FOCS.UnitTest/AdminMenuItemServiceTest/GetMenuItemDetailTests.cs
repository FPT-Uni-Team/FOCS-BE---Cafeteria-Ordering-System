using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.AdminMenuItemServiceTest
{
    public class GetMenuItemDetailTests : AdminMenuItemServiceTestBase
    {
        [Fact]
        public async Task GetMenuDetailAsync_ShouldReturnDto_WhenItemExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var menuItem = CreateValidMenuItem(Guid.NewGuid());
            menuItem.Id = id;

            _menuRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem> { menuItem });

            var expectedDto = CreateValidMenuItemAdminDTO(menuItem.StoreId);
            expectedDto.Id = id;

            _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(menuItem)).Returns(expectedDto);

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldReturnNull_WhenItemDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _menuRepositoryMock.Setup(x => x.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem>()); // empty

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldReturnNull_WhenItemIsDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var deletedItem = CreateValidMenuItem(Guid.NewGuid());
            deletedItem.Id = id;
            deletedItem.IsDeleted = true;

            _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem> { deletedItem });

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldReturnNull_WhenMappedItemIsNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem> { item });

            _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(item)).Returns((MenuItemAdminDTO?)null);

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldCallMapper_Once_WhenItemFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var item = CreateValidMenuItem(Guid.NewGuid());
            item.Id = id;

            _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem> { item });

            _mapperMock.Setup(m => m.Map<MenuItemAdminDTO>(item)).Returns(new MenuItemAdminDTO { Id = item.Id });

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            _mapperMock.Verify(m => m.Map<MenuItemAdminDTO>(item), Times.Once);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldNotCallMapper_WhenItemNotFound()
        {
            // Arrange
            _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .ReturnsAsync(new List<MenuItem>()); // No item

            // Act
            var result = await _adminMenuItemService.GetMenuDetailAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
            _mapperMock.Verify(m => m.Map<MenuItemAdminDTO>(It.IsAny<MenuItem>()), Times.Never);
        }

        [Fact]
        public async Task GetMenuDetailAsync_ShouldUseCorrectFilterPredicate()
        {
            // Arrange
            var id = Guid.NewGuid();
            Expression<Func<MenuItem, bool>>? capturedPredicate = null;

            _menuRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MenuItem, bool>>>()))
                .Callback<Expression<Func<MenuItem, bool>>>(expr => capturedPredicate = expr)
                .ReturnsAsync(new List<MenuItem>());

            // Act
            await _adminMenuItemService.GetMenuDetailAsync(id);

            // Assert
            Assert.NotNull(capturedPredicate);

            var testItem = new MenuItem { Id = id, IsDeleted = false };
            var result = capturedPredicate!.Compile().Invoke(testItem);

            Assert.True(result); // should match
        }

    }
}

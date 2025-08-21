using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CategoryServiceTest
{
    public class CreateCategoryTests : CategoryServiceTestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateCategoryAsync_ShouldThrowArgumentException_WhenStoreIdNullOrWhitespace(string storeId)
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Cat", Description = "Desc", SortOrder = 1 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _categoryService.CreateCategoryAsync(request, storeId));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowArgumentException_WhenStoreIdNotGuid()
        {
            // Arrange
            var request = new CreateCategoryRequest { Name = "Cat", Description = "Desc", SortOrder = 1 };
            var invalidStoreId = "not-a-guid";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _categoryService.CreateCategoryAsync(request, invalidStoreId));
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldThrowException_WhenCategoryNameExists()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var request = new CreateCategoryRequest
            {
                Name = "Existing",
                Description = "Desc",
                SortOrder = 1
            };
            var existing = new List<Category>
            {
                new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Existing",
                    StoreId = Guid.Parse(storeId)
                }
            };

            SetupFindCategories(existing);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _categoryService.CreateCategoryAsync(request, storeId));

            AssertConditionException(ex, Errors.Category.CategoryIsExist);
        }


        [Fact]
        public async Task CreateCategoryAsync_ShouldAddAndReturnDto_WhenValid()
        {
            // Arrange
            var storeId = Guid.NewGuid().ToString();
            var parsedStoreId = Guid.Parse(storeId);
            var request = new CreateCategoryRequest { Name = "NewCat", Description = "Desc", SortOrder = 5 };
            // No existing categories
            SetupFindCategories(new List<Category>());

            // Prepare mapping
            _mapperMock.Setup(m => m.Map<Category>(request))
                .Returns(new Category
                {
                    Name = request.Name,
                    Description = request.Description,
                    SortOrder = request.SortOrder
                });

            _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(It.IsAny<Category>()))
                .Returns((Category src) => new MenuCategoryDTO
                {
                    Id = src.Id,
                    Name = src.Name,
                    Description = src.Description,
                    IsActive = src.IsActive
                });

            Category added = null!;
            _categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(c => added = c)
                .Returns(Task.CompletedTask);
            _categoryRepositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _categoryService.CreateCategoryAsync(request, storeId);

            // Assert
            // Verify repository Add and SaveChanges
            _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
            _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            // Check generated values
            Assert.NotNull(added);
            Assert.Equal(parsedStoreId, added.StoreId);
            Assert.Equal(request.Name, added.Name);
            Assert.Equal(request.Description, added.Description);
            Assert.NotEqual(Guid.Empty, added.Id);

            // Check returned DTO
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
            Assert.Equal(added.Name, result.Name);
            Assert.Equal(added.Description, result.Description);
            Assert.True(result.IsActive);
        }
    }
}

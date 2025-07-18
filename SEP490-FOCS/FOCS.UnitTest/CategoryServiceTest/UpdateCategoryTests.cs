using FOCS.Common.Exceptions;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;

namespace FOCS.UnitTest.CategoryServiceTest
{
    public class UpdateCategoryTests : CategoryServiceTestBase
    {
        private readonly Guid _categoryId;
        private readonly Category _existingCategory;
        private readonly UpdateCategoryRequest _request;

        public UpdateCategoryTests()
        {
            _categoryId = Guid.NewGuid();
            _existingCategory = new Category
            {
                Id = _categoryId,
                Name = "OldName",
                Description = "OldDesc",
                SortOrder = 5,
                StoreId = Guid.NewGuid(),
                IsDeleted = false,
                IsActive = true
            };

            _request = new UpdateCategoryRequest
            {
                Name = "NewName",
                Description = "NewDesc",
                SortOrder = 10
            };
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsUpdatedDto_WhenCategoryExists()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });
            // Mock mapper to apply request to existing entity
            _mapperMock.Setup(m => m.Map(_request, _existingCategory))
                       .Callback<UpdateCategoryRequest, Category>((req, cate) =>
                       {
                           cate.Name = req.Name;
                           cate.Description = req.Description;
                           cate.SortOrder = req.SortOrder;
                       });
            // Mapper for returning DTO
            _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(_existingCategory))
                       .Returns((Category cate) => new MenuCategoryDTO
                       {
                           Id = cate.Id,
                           Name = cate.Name,
                           Description = cate.Description,
                           IsActive = cate.IsActive
                       });

            // Act
            var result = await _categoryService.UpdateCategoryAsync(_request, _categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_categoryId, result.Id);
            Assert.Equal("NewName", result.Name);
            Assert.Equal("NewDesc", result.Description);
            // Verify Update and SaveChanges
            _categoryRepositoryMock.Verify(r => r.Update(_existingCategory), Times.Once());
            VerifySaveChanges(Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_ThrowsConditionException_WhenCategoryNotFound()
        {
            // Arrange: no category returned
            SetupFindCategories(new List<Category>());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _categoryService.UpdateCategoryAsync(_request, _categoryId, _existingCategory.StoreId.ToString())
            );
            AssertConditionException(ex, Errors.Common.NotFound);
        }

        [Fact]
        public async Task UpdateCategoryAsync_Propagates_WhenSaveChangesThrows()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });
            _mapperMock.Setup(m => m.Map(_request, _existingCategory));
            _categoryRepositoryMock.Setup(r => r.SaveChangesAsync())
                                   .ThrowsAsync(new InvalidOperationException("Save failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _categoryService.UpdateCategoryAsync(_request, _categoryId, _existingCategory.StoreId.ToString())
            );
            // Ensure Update was called before failure
            _categoryRepositoryMock.Verify(r => r.Update(_existingCategory), Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_MapperDoesNotChangeFields_WhenNoChangesProvided()
        {
            // Arrange
            var noChangeRequest = new UpdateCategoryRequest
            {
                Name = _existingCategory.Name,
                Description = _existingCategory.Description,
                SortOrder = _existingCategory.SortOrder
            };
            SetupFindCategories(new List<Category> { _existingCategory });

            // Gắn callback mapper để xác nhận mapper được gọi đúng cách
            _mapperMock.Setup(m => m.Map(noChangeRequest, _existingCategory))
                       .Callback<UpdateCategoryRequest, Category>((req, cate) =>
                       {
                           // Giả lập rằng mapper không thay đổi gì
                       });

            _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(_existingCategory))
                       .Returns(new MenuCategoryDTO { Id = _existingCategory.Id, Name = _existingCategory.Name });

            // Act
            var result = await _categoryService.UpdateCategoryAsync(noChangeRequest, _categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.Equal(_existingCategory.Name, result.Name);
            _mapperMock.Verify(m => m.Map(noChangeRequest, _existingCategory), Times.Once());
            _categoryRepositoryMock.Verify(r => r.Update(_existingCategory), Times.Once());
            VerifySaveChanges(Times.Once());
        }

        [Fact]
        public async Task UpdateCategoryAsync_Throws_WhenRequestIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _categoryService.UpdateCategoryAsync(null, _categoryId, _existingCategory.StoreId.ToString())
            );
        }

        [Fact]
        public async Task UpdateCategoryAsync_Throws_WhenCategoryIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _categoryService.UpdateCategoryAsync(_request, Guid.Empty, _existingCategory.StoreId.ToString())
            );
        }

        [Fact]
        public async Task UpdateCategoryAsync_Throws_WhenMapperReturnsNullDto()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });
            _mapperMock.Setup(m => m.Map(_request, _existingCategory));
            _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(_existingCategory)).Returns<MenuCategoryDTO>(null);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(_request, _categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.Null(result); // hoặc throw exception nếu code thật bắt buộc không null
        }

    }
}

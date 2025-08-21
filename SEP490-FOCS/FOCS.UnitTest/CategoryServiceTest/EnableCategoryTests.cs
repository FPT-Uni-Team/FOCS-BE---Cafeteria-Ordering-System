using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FOCS.Application.Services;
using FOCS.Common.Constants;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using Xunit;

namespace FOCS.UnitTest.CategoryServiceTest
{
    public class EnableCategoryTests : CategoryServiceTestBase
    {
        private readonly Guid _categoryId;
        private readonly Category _existingCategory;

        public EnableCategoryTests()
        {
            _categoryId = Guid.NewGuid();
            _existingCategory = new Category
            {
                Id = _categoryId,
                Name = "TestCat",
                Description = "Desc",
                SortOrder = 1,
                StoreId = Guid.NewGuid(),
                IsDeleted = false,
                IsActive = false // initially disabled
            };
        }

        [Fact]
        public async Task EnableCategory_ReturnsTrue_WhenCategoryExists()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });

            // Act
            var result = await _categoryService.EnableCategory(_categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.True(result);
            Assert.True(_existingCategory.IsActive);
            _categoryRepositoryMock.Verify(r => r.Update(_existingCategory), Times.Once());
            VerifySaveChanges(Times.Once());
        }

        [Fact]
        public async Task EnableCategory_ReturnsFalse_WhenCategoryNotFound()
        {
            // Arrange
            SetupFindCategories(new List<Category>());

            // Act
            var result = await _categoryService.EnableCategory(_categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.False(result);
            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never());
            VerifySaveChanges(Times.Never());
        }

        [Fact]
        public async Task EnableCategory_ReturnsFalse_WhenSaveChangesThrows()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });
            _categoryRepositoryMock
                .Setup(r => r.SaveChangesAsync())
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var result = await _categoryService.EnableCategory(_categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.False(result);
            Assert.True(_existingCategory.IsActive);
            _categoryRepositoryMock.Verify(r => r.Update(_existingCategory), Times.Once());
            VerifySaveChanges(Times.Once());
        }

        [Fact]
        public async Task EnableCategory_ReturnsFalse_WhenFindThrows()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ThrowsAsync(new Exception("Find error"));

            // Act
            var result = await _categoryService.EnableCategory(_categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.False(result);
            _categoryRepositoryMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Never());
            VerifySaveChanges(Times.Never());
        }

        [Fact]
        public async Task EnableCategory_ReturnsFalse_WhenUpdateThrows()
        {
            // Arrange
            SetupFindCategories(new List<Category> { _existingCategory });
            _categoryRepositoryMock
                .Setup(r => r.Update(It.IsAny<Category>()))
                .Throws(new Exception("Update error"));

            // Act
            var result = await _categoryService.EnableCategory(_categoryId, _existingCategory.StoreId.ToString());

            // Assert
            Assert.False(result);
            VerifySaveChanges(Times.Never());
        }
    }
}

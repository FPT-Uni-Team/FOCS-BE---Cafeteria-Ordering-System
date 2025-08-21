using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest.CategoryServiceTest
{
    public class CategoryServiceTestBase
    {
        protected readonly Mock<IRepository<Category>> _categoryRepositoryMock;
        protected readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        protected readonly Mock<IMapper> _mapperMock;
        protected readonly CategoryService _categoryService;

        protected CategoryServiceTestBase()
        {
            _categoryRepositoryMock = new Mock<IRepository<Category>>();
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _mapperMock = new Mock<IMapper>();

            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _menuItemRepositoryMock.Object
            );
        }

        protected void SetupCategoryQueryable(List<Category> categories)
        {
            var mockSet = categories.AsQueryable().BuildMockDbSet().Object;
            _categoryRepositoryMock
                .Setup(r => r.AsQueryable())
                .Returns(mockSet);

            _mapperMock
                .Setup(m => m.Map<MenuCategoryDTO>(It.IsAny<Category>()))
                .Returns((Category src) => new MenuCategoryDTO
                {
                    Id = src.Id,
                    Name = src.Name,
                    Description = src.Description,
                    IsActive = src.IsActive
                });

            _mapperMock
                .Setup(m => m.Map<List<MenuCategoryDTO>>(It.IsAny<List<Category>>()))
                .Returns((List<Category> src) => src.Select(c => new MenuCategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive
                }).ToList());
        }

        protected void SetupFindCategories(List<Category> categories)
        {
            _categoryRepositoryMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                .ReturnsAsync(categories);
        }

        protected void VerifySaveChanges(Times times)
        {
            _categoryRepositoryMock.Verify(r => r.SaveChangesAsync(), times);
        }

        protected void AssertConditionException(Exception ex, string expectedMessage, string? expectedField = null)
        {
            Assert.NotNull(ex);
            var parts = ex.Message.Split('@');

            Assert.Equal(expectedMessage, parts[0]);

            if (expectedField != null)
            {
                Assert.Equal(2, parts.Length);
                Assert.Equal(expectedField, parts[1]);
            }
            else
            {
                // field may be null or empty
                Assert.True(parts.Length >= 1);
            }
        }
    }
}

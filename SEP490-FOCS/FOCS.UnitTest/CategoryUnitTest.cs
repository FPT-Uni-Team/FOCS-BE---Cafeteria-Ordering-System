using AutoMapper;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Order.Infrastucture.Entities;
using MockQueryable.Moq;
using Moq;
using System.Linq.Expressions;

namespace FOCS.UnitTest
{
    public class CategoryUnitTest
    {
        private readonly Mock<IRepository<Category>> _categoryRepositoryMock;
        private readonly Mock<IRepository<MenuItem>> _menuItemRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CategoryService _categoryService;

        public CategoryUnitTest()
        {
            _categoryRepositoryMock = new Mock<IRepository<Category>>();
            _menuItemRepositoryMock = new Mock<IRepository<MenuItem>>();
            _mapperMock = new Mock<IMapper>();

            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _menuItemRepositoryMock.Object);
        }

        #region CreateCategory CM-26
        [Theory]
        [InlineData("String Name", "String Description", 1, true)]
        [InlineData(null, "String Description", 1, false)]
        [InlineData("String Name", null, 1, false)]
        [InlineData("String Name", "String Description", null, false)]
        [InlineData(null, null, null, false)]
        public async Task CreateCategoryAsync_SimpleRun_ChecksIfServiceRuns(
            string name,
            string description,
            int? sortOrder,
            bool shouldSucceed)
        {
            var storeIdStr = Guid.NewGuid().ToString();

            var request = new CreateCategoryRequest
            {
                Name = name,
                Description = description,
                SortOrder = sortOrder ?? 0
            };

            if (shouldSucceed)
            {
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category>());

                _mapperMock.Setup(m => m.Map<Category>(It.IsAny<CreateCategoryRequest>()))
                    .Returns((CreateCategoryRequest req) => new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = req.Name,
                        Description = req.Description,
                        SortOrder = req.SortOrder,
                        StoreId = Guid.Parse(storeIdStr)
                    });

                _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(It.IsAny<Category>()))
                    .Returns((Category c) => new MenuCategoryDTO { Id = c.Id, Name = c.Name, Description = c.Description });

                _categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
                _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category> { new Category { Id = Guid.NewGuid(), Name = name ?? "X", StoreId = Guid.TryParse(storeIdStr, out var ps) ? ps : Guid.NewGuid() } });
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _categoryService.CreateCategoryAsync(request, storeIdStr);
            });

            if (shouldSucceed)
                Assert.Null(ex);
            else
                Assert.NotNull(ex);
        }
        #endregion

        #region ListCategories CM-23
        [Theory]
        [InlineData(1, 5, "name", "Chicken", "name", "desc", true)]
        [InlineData(1, 5, "description", "Chicken", "name", "desc", true)]
        [InlineData(1, 5, null, "Chicken", "name", "desc", true)]
        [InlineData(1, 5, "name", null, "name", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "base_price", "desc", true)]
        [InlineData(1, 5, "name", "Chicken", null, "desc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", "asc", true)]
        [InlineData(1, 5, "name", "Chicken", "name", null, true)]
        //[InlineData(null, 5, "name", "Chicken", "name", null, false)]
        //[InlineData(1, null, "name", "Chicken", "name", null, false)]
        public async Task ListCategoriesAsync_SimpleRun_ChecksIfServiceRuns(
            int? page,
            int? pageSize,
            string? searchBy,
            string? searchValue,
            string? sortBy,
            string? sortOrder,
            bool shouldSucceed)
        {
            // Arrange
            var storeIdStr = Guid.NewGuid().ToString();
            Guid storeId = Guid.Parse(storeIdStr);

            var query = new UrlQueryParameters
            {
                Page = page ?? 0,
                PageSize = pageSize ?? 0,
                SearchBy = searchBy,
                SearchValue = searchValue,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            if (shouldSucceed)
            {
                var cats = new List<Category>
                {
                    new Category { Id = Guid.NewGuid(), Name = "Chicken", Description = "Chicken", StoreId = storeId, IsDeleted = false },
                    new Category { Id = Guid.NewGuid(), Name = "Beef", Description = "Beef", StoreId = storeId, IsDeleted = false }
                };
                _categoryRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(cats.AsQueryable().BuildMockDbSet().Object);

                _mapperMock.Setup(m => m.Map<List<MenuCategoryDTO>>(It.IsAny<List<Category>>()))
                    .Returns((List<Category> src) => src.Select(c => new MenuCategoryDTO { Id = c.Id, Name = c.Name }).ToList());
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Category>().AsQueryable().BuildMockDbSet().Object);
            }

            // Act
            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _categoryService.ListCategoriesAsync(query, storeIdStr);
            });

            // Assert
            if (shouldSucceed) Assert.Null(ex); else Assert.NotNull(ex);
        }
        #endregion

        #region DisableCategory / EnableCategory : CM-30 / CM-29
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData(null, false)]
        public async Task DisableCategory_SimpleRun_ChecksIfServiceRuns(string categoryIdStr, bool shouldSucceed)
        {
            // Arrange
            Guid categoryId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(categoryIdStr))
                categoryId = Guid.Parse(categoryIdStr);

            if (shouldSucceed)
            {
                var cate = new Category { Id = categoryId, IsActive = true, IsDeleted = false };
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category> { cate });
                _categoryRepositoryMock.Setup(r => r.Update(It.IsAny<Category>()));
                _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category>());
            }

            // Act
            var result = await _categoryService.DisableCategory(categoryId, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(shouldSucceed, result);
        }

        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData(null, false)]
        public async Task EnableCategory_SimpleRun_ChecksIfServiceRuns(string categoryIdStr, bool shouldSucceed)
        {
            // Arrange
            Guid categoryId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(categoryIdStr))
                categoryId = Guid.Parse(categoryIdStr);

            if (shouldSucceed)
            {
                var cate = new Category { Id = categoryId, IsActive = false, IsDeleted = false };
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category> { cate });
                _categoryRepositoryMock.Setup(r => r.Update(It.IsAny<Category>()));
                _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category>());
            }

            // Act
            var result = await _categoryService.EnableCategory(categoryId, Guid.NewGuid().ToString());

            // Assert
            Assert.Equal(shouldSucceed, result);
        }
        #endregion

        #region UpdateCategory CM-28
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 1, "550e8400-e29b-41d4-a716-446655440000", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", "String Name", "String Description", 1, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData(null, "String Name", "String Description", 1, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", null, "String Description", 1, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", null, 1, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", null, "550e8400-e29b-41d4-a716-446655440000", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 1, "fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", "String Name", "String Description", 1, null, false)]
        [InlineData(null, null, null, null, null, false)]
        public async Task UpdateCategoryAsync_SimpleRun_ChecksIfServiceRuns(
            string categoryIdStr,
            string name,
            string description,
            int? sortOrder,
            string storeIdStr,
            bool shouldSucceed)
        {
            // Arrange & parse
            Guid categoryId = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(categoryIdStr))
                categoryId = Guid.Parse(categoryIdStr);

            var request = new UpdateCategoryRequest
            {
                Name = name,
                Description = description,
                SortOrder = sortOrder ?? 0
            };

            if (shouldSucceed)
            {
                // existing found
                var existing = new Category { Id = categoryId, Name = "Old", StoreId = Guid.Parse(storeIdStr) };
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category> { existing });

                _mapperMock.Setup(m => m.Map(It.IsAny<UpdateCategoryRequest>(), It.IsAny<Category>()))
                    .Callback<UpdateCategoryRequest, Category>((req, cat) =>
                    {
                        cat.Name = req.Name;
                        cat.Description = req.Description;
                        cat.SortOrder = req.SortOrder;
                    });

                _categoryRepositoryMock.Setup(r => r.Update(It.IsAny<Category>()));
                _categoryRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

                _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(It.IsAny<Category>()))
                    .Returns((Category c) => new MenuCategoryDTO { Id = c.Id, Name = c.Name, Description = c.Description });
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Category, bool>>>()))
                    .ReturnsAsync(new List<Category>());
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _categoryService.UpdateCategoryAsync(request, categoryId, storeIdStr);
            });

            if (shouldSucceed) Assert.Null(ex); else Assert.NotNull(ex);
        }
        #endregion

        #region GetById CM-27
        [Theory]
        [InlineData("8172b0b1-8656-4841-ac2e-24034b9249ed", true)]
        [InlineData("fb05206f-1188-432c-9e5c-4e7094d5b84d", false)]
        [InlineData(null, false)]
        public async Task GetById_SimpleRun_ChecksIfServiceRuns(string idStr, bool shouldSucceed)
        {
            Guid id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(idStr))
                id = Guid.Parse(idStr);

            var storeId = Guid.NewGuid();

            if (shouldSucceed)
            {
                var cate = new Category { Id = id, StoreId = storeId, Name = "Cat1" };
                _categoryRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Category> { cate }.AsQueryable().BuildMockDbSet().Object);
                _mapperMock.Setup(m => m.Map<MenuCategoryDTO>(It.IsAny<Category>()))
                    .Returns((Category c) => new MenuCategoryDTO { Id = c.Id, Name = c.Name });
            }
            else
            {
                _categoryRepositoryMock.Setup(r => r.AsQueryable())
                    .Returns(new List<Category>().AsQueryable().BuildMockDbSet().Object);
            }

            Exception ex = await Record.ExceptionAsync(async () =>
            {
                var res = await _categoryService.GetById(id, storeId);
                if (shouldSucceed) Assert.NotNull(res); else Assert.Null(res);
            });

            Assert.Null(ex);
        }
        #endregion
    }
}

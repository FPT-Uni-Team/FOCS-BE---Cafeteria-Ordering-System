using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FOCS.Application.Services;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using Moq;
using Xunit;

namespace FOCS.UnitTest.CategoryServiceTest
{
    public class ListCategoriesTests : CategoryServiceTestBase
    {
        private readonly Guid _storeGuid;
        private readonly string _storeId;
        private readonly List<Category> _allCategories;

        public ListCategoriesTests()
        {
            // Prepare common storeId and categories
            _storeGuid = Guid.NewGuid();
            _storeId = _storeGuid.ToString();

            _allCategories = new List<Category>
            {
                new Category { Id = Guid.NewGuid(), Name = "Alpha", Description = "First Desc", SortOrder = 2, StoreId = _storeGuid, IsDeleted = false, IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Bravo", Description = "Second Desc", SortOrder = 1, StoreId = _storeGuid, IsDeleted = false, IsActive = true },
                new Category { Id = Guid.NewGuid(), Name = "Charlie", Description = "Third Desc", SortOrder = 3, StoreId = _storeGuid, IsDeleted = false, IsActive = true },
                // filtered out: deleted
                new Category { Id = Guid.NewGuid(), Name = "Delta", Description = "Fourth Desc", SortOrder = 4, StoreId = _storeGuid, IsDeleted = true, IsActive = true },
                // filtered out: different store
                new Category { Id = Guid.NewGuid(), Name = "Echo", Description = "Fifth Desc", SortOrder = 5, StoreId = Guid.NewGuid(), IsDeleted = false, IsActive = true }
            };
        }

        [Fact]
        public async Task ListCategoriesAsync_PaginatesCorrectly()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 2, PageSize = 1 };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.PageIndex);
            Assert.Equal(1, result.PageSize);
            Assert.Single(result.Items);
            Assert.Equal("Bravo", result.Items[0].Name);
        }

        [Fact]
        public async Task ListCategoriesAsync_NoSearch_ReturnsAllValid()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SearchBy = null, SearchValue = null };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var names = result.Items.Select(i => i.Name).ToArray();

            Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, names);
        }

        [Fact]
        public async Task ListCategoriesAsync_UnknownSearchBy_Ignored()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SearchBy = "unknown", SearchValue = "anything" };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var names = result.Items.Select(i => i.Name).ToArray();

            Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, names);
        }

        [Fact]
        public async Task ListCategoriesAsync_SearchByName_Filters()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SearchBy = "name", SearchValue = "ha" }; // Alpha, Charlie

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var names = result.Items.Select(i => i.Name).OrderBy(n => n);
            Assert.Equal(new[] { "Alpha", "Charlie" }, names);
        }

        [Fact]
        public async Task ListCategoriesAsync_SearchByDescription_Filters()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SearchBy = "description", SearchValue = "thi" }; // "Third Desc"

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            Assert.Single(result.Items);
            Assert.Equal("Charlie", result.Items[0].Name);
        }

        [Fact]
        public async Task ListCategoriesAsync_NoSort_ReturnsInInsertionOrder()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SortBy = null, SortOrder = null };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var names = result.Items.Select(i => i.Name).ToArray();

            Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, names);
        }

        [Fact]
        public async Task ListCategoriesAsync_UnknownSortBy_Ignored()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 10, SortBy = "unknown", SortOrder = "desc" };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var names = result.Items.Select(i => i.Name).ToArray();

            Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, names);
        }

        [Theory]
        [InlineData("name", "asc", new[] { "Alpha", "Bravo", "Charlie" })]
        [InlineData("name", "desc", new[] { "Charlie", "Bravo", "Alpha" })]
        [InlineData("sort_order", "asc", new[] { "Bravo", "Alpha", "Charlie" })]
        [InlineData("sort_order", "desc", new[] { "Charlie", "Alpha", "Bravo" })]
        public async Task ListCategoriesAsync_Sorting_Works(string sortBy, string sortOrder, string[] expected)
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _categoryService.ListCategoriesAsync(query, _storeId);
            var actual = result.Items.Select(i => i.Name).ToArray();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ListCategoriesAsync_InvalidStoreId_ThrowsWrappedFormatException()
        {
            SetupCategoryQueryable(_allCategories);
            var query = new UrlQueryParameters { Page = 1, PageSize = 5 };

            var ex = await Assert.ThrowsAsync<TargetInvocationException>(
                () => _categoryService.ListCategoriesAsync(query, "invalid-guid")
            );
            Assert.IsType<FormatException>(ex.InnerException);
        }
    }
}

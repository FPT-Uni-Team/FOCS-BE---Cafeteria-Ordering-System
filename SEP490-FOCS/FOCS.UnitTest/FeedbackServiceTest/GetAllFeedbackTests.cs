using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;

namespace FOCS.UnitTest.FeedbackServiceTest
{
    public class GetAllFeedbackTests
    {
        private readonly List<Feedback> _testData = new List<Feedback>
        {
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 5,
                Comment = "Excellent service! Highly recommended!",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 3,
                Comment = "Average experience",
                IsPublic = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 4,
                Comment = "Good service overall",
                IsPublic = true,
                CreatedAt = DateTime.UtcNow
            },
            new Feedback
            {
                Id = Guid.NewGuid(),
                Rating = 5,
                Comment = null, // Test null comment
                IsPublic = true,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        [Theory]
        [InlineData("rating", "5", 2)]
        [InlineData("rating", "3", 1)]
        [InlineData("is_public", "", 3)]
        public void ApplyFilters_ShouldFilterCorrectly(string filterKey, string filterValue, int expectedCount)
        {
            // Arrange
            var parameters = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { filterKey, filterValue } }
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplyFilters(query, parameters).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public void ApplyFilters_ShouldFilterByCreatedDateFrom()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-1.5).ToString("o"); // should match last 3 items
            var parameters = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { "created_date_from", fromDate } }
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplyFilters(query, parameters).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, r => Assert.True(r.CreatedAt > DateTime.Parse(fromDate)));
        }

        [Fact]
        public void ApplyFilters_ShouldFilterByCreatedDateTo()
        {
            // Arrange
            var toDate = DateTime.UtcNow.AddDays(-1).ToString("o"); // should match first item only
            var parameters = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string> { { "created_date_to", toDate } }
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplyFilters(query, parameters).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.True(r.CreatedAt < DateTime.Parse(toDate)));
        }

        [Fact]
        public void ApplyFilters_ShouldFilterByDateRange()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-2).ToString("o");
            var toDate = DateTime.UtcNow.ToString("o");
            var parameters = new UrlQueryParameters
            {
                Filters = new Dictionary<string, string>
                {
                    { "created_date_from", fromDate },
                    { "created_date_to", toDate }
                }
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplyFilters(query, parameters).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, r =>
                Assert.True(r.CreatedAt > DateTime.Parse(fromDate) && r.CreatedAt < DateTime.Parse(toDate)));
        }

        [Fact]
        public void ApplyFilters_ShouldReturnAll_WhenNoFilters()
        {
            // Arrange
            var parameters = new UrlQueryParameters();
            var query = _testData.AsQueryable();

            // Act
            var result = ApplyFilters(query, parameters).ToList();

            // Assert
            Assert.Equal(_testData.Count, result.Count);
        }

        [Theory]
        [InlineData("comment", "excellent", 1)]
        [InlineData("comment", "service", 2)]
        [InlineData("comment", "nonexistent", 0)]
        [InlineData("invalid_field", "test", 4)] // Should return all
        public void ApplySearch_ShouldSearchCorrectly(string searchBy, string searchValue, int expectedCount)
        {
            // Arrange
            var parameters = new UrlQueryParameters
            {
                SearchBy = searchBy,
                SearchValue = searchValue
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplySearch(query, parameters).ToList();

            // Assert
            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public void ApplySearch_ShouldHandleNullComment()
        {
            // Arrange
            var parameters = new UrlQueryParameters
            {
                SearchBy = "comment",
                SearchValue = "test"
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplySearch(query, parameters).ToList();

            // Assert
            // Should not throw and should exclude null comments
            Assert.DoesNotContain(result, f => f.Comment == null);
        }

        [Theory]
        [InlineData("created_date", "asc")]
        [InlineData("created_date", "desc")]
        [InlineData("rating", "asc")]
        [InlineData("rating", "desc")]
        public void ApplySort_ShouldSortCorrectly(string sortBy, string sortOrder)
        {
            // Arrange
            var parameters = new UrlQueryParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var query = _testData.AsQueryable();

            // Act
            var result = ApplySort(query, parameters).ToList();

            // Assert
            if (sortBy == "created_date")
            {
                var expected = sortOrder == "asc"
                    ? _testData.OrderBy(f => f.CreatedAt)
                    : _testData.OrderByDescending(f => f.CreatedAt);

                Assert.Equal(expected.Select(f => f.Id), result.Select(f => f.Id));
            }
            else if (sortBy == "rating")
            {
                var expected = sortOrder == "asc"
                    ? _testData.OrderBy(f => f.Rating)
                    : _testData.OrderByDescending(f => f.Rating);

                Assert.Equal(expected.Select(f => f.Id), result.Select(f => f.Id));
            }
        }

        [Fact]
        public void ApplySort_ShouldReturnDefaultOrder_WhenNoSortSpecified()
        {
            // Arrange
            var parameters = new UrlQueryParameters();
            var query = _testData.AsQueryable();

            // Act
            var result = ApplySort(query, parameters).ToList();

            // Assert
            // Should return in original order
            Assert.Equal(_testData.Select(f => f.Id), result.Select(f => f.Id));
        }

        private static IQueryable<Feedback> ApplyFilters(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    "rating" => query.Where(p => p.Rating == int.Parse(value)),
                    "is_public" => query.Where(p => p.IsPublic),
                    "created_date_from" => query.Where(p => p.CreatedAt > DateTime.Parse(value)),
                    "created_date_to" => query.Where(p => p.CreatedAt < DateTime.Parse(value)),
                    _ => query
                };
            }

            return query;
        }

        private static IQueryable<Feedback> ApplySearch(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) ||
                string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "comment" => query.Where(p => p.Comment != null && p.Comment.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<Feedback> ApplySort(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SortBy))
                return query;

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return parameters.SortBy.ToLowerInvariant() switch
            {
                "created_date" => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                "rating" => isDescending
                    ? query.OrderByDescending(p => p.Rating)
                    : query.OrderBy(p => p.Rating),
                _ => query
            };
        }
    }
}
using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuInsightService
    {
        /// <summary>
        /// Get a list of product which is best order in period of time
        /// </summary>
        /// <param name="since">Period of time where (ex: In the previous 3 days)</param>
        /// <param name="topN">Count of return product</param>
        Task<List<MenuItemInsightResponse>> GetMostOrderedProductsAsync(TimeSpan since, string storeId, int topN = 10);

        /// <summary>
        /// Suggest based on history ordered of user
        /// </summary>
        /// <param name="userId">ID user</param>
        /// <param name="topN">Count of suggest product</param>
        Task<List<MenuItemInsightResponse>> GetSuggestedProductsBasedOnHistoryAsync(Guid userId, int topN = 10);

        /// <summary>
        /// Suggest based on the best current promotion
        /// </summary>
        /// <param name="topN">Count of products which admin want to show</param>
        Task<List<MenuItemInsightResponse>> GetProductsBasedOnBestPromotionAsync(string storeId, int topN = 10);
    }
}

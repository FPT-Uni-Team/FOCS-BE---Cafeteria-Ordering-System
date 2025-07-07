using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemCategoryService
    {
        #region category
        Task<bool> AssignMenuItemsToCategory(Guid cateId, List<Guid> menuItemIds, Guid storeId);

        Task<CategoryMenuItemDetailResponse> GetCategoryWithMenuItemDetail(Guid cateId, Guid StoreId);

        Task<PagedResult<CategoryMenuItemDetailResponse>> ListCategoriesWithMenuItems(UrlQueryParameters urlQueryParameters, Guid storeId);

        Task<bool> RemoveMenuItemFromCategory(Guid cateId, List<Guid> menuItemIds);

        #endregion

        #region menu item
        Task AssignCategoriesToMenuItem(List<Guid> categoryIds, Guid menuItemId, string StoreId);
        Task<List<MenuCategoryDTO>> ListCategoriesWithMenuItem(Guid menuItemId, string storeId);
        Task<bool> RemoveCategoriesFromProduct(RemoveCategoriesFromProductRequest removeCategoriesFromProductRequest, string storeId);

        #endregion
    }
}

using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface ICategoryService
    {
        Task<MenuCategoryDTO> CreateCategoryAsync(CreateCategoryRequest request, string? storeId);
        Task<MenuCategoryDTO> UpdateCategoryAsync(UpdateCategoryRequest request, Guid categoryId, string? storeId);
        Task<bool> DisableCategory(Guid categoryId, string? storeId);
        Task<bool> EnableCategory(Guid categoryId, string? storeId);
        Task<PagedResult<MenuCategoryDTO>> ListCategoriesAsync(UrlQueryParameters queryParameters, string? storeId);
        Task<MenuCategoryDTO> GetById(Guid Id, Guid StoreId);

        Task<bool> RemoveCategory(Guid id, string storeId);
    }
}

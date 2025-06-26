using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminMenuItemService
    {
        Task<MenuItemAdminDTO> CreateMenuAsync(MenuItemAdminDTO dto, string userId);
        Task<PagedResult<MenuItemAdminDTO>> GetAllMenuItemAsync(UrlQueryParameters query, Guid storeId);
        Task<MenuItemAdminDTO?> GetMenuDetailAsync(Guid id);
        Task<bool> UpdateMenuAsync(Guid id, MenuItemAdminDTO dto, string userId);
        Task<bool> DeleteMenuAsync(Guid id, string userId);
        Task<MenuItemDetailAdminDTO> GetMenuItemDetail(Guid menuItemId, string StoreId);
        Task<List<MenuItemDetailAdminDTO>> GetListMenuItemDetail(List<Guid> menuItemIds, string storeId, string userId);
    }
}

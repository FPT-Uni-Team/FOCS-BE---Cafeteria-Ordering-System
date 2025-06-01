using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IMenuManagementService
    {
        Task<MenuItemAdminServiceDTO> CreateMenuAsync(MenuItemAdminServiceDTO dto, string userId);
        Task<PagedResult<MenuItemAdminServiceDTO>> GetAllMenuItemAsync(UrlQueryParameters query, Guid storeId);
        Task<MenuItemAdminServiceDTO?> GetMenuDetailAsync(Guid id);
        Task<bool> UpdateMenuAsync(Guid id, MenuItemAdminServiceDTO dto, string userId);
        Task<bool> DeleteMenuAsync(Guid id, string userId);
        Task<MenuItemDetailAdminServiceDTO> GetMenuItemDetail(Guid menuItemId, string StoreId);
    }
}

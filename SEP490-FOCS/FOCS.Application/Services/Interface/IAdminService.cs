using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Models;

namespace FOCS.Application.Services.Interface
{
    public interface IAdminService
    {
        Task<MenuItemAdminServiceDTO> CreateMenuAsync(MenuItemAdminServiceDTO dto, string userId);
        Task<PagedResult<MenuItemAdminServiceDTO>> GetAllMenusAsync(UrlQueryParameters query, Guid storeId);
        Task<MenuItemAdminServiceDTO?> GetMenuDetailAsync(Guid id);
        Task<bool> UpdateMenuAsync(Guid id, MenuItemAdminServiceDTO dto, string userId);
        Task<bool> DeleteMenuAsync(Guid id, string userId);
    }
}

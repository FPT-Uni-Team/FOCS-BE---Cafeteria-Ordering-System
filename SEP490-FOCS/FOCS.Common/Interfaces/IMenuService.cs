using FOCS.Common.Models;

namespace FOCS.Order.Infrastucture.Interfaces
{
    public interface IMenuService
    {
        Task<PagedResult<MenuItemDTO>> GetMenuItemByStore(UrlQueryParameters urlQueryParameters, Guid storeId);
        Task<MenuItemDTO> GetItemVariant(Guid itemId);
    }
}

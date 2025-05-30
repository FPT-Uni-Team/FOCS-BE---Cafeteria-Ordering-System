using FOCS.Order.Infrastucture.Entities;

namespace FOCS.Order.Infrastucture.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuItemVariant>> GetMenuByStore(Guid storeId);
    }
}

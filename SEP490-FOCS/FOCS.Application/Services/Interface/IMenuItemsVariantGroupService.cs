using FOCS.Application.DTOs;
using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemsVariantGroupService
    {
        Task<List<MenuItemVariantGroup>> AssignMenuItemToVariantGroup(CreateMenuItemVariantGroupRequest request);

        Task<List<VariantGroupResponse>> GetVariantGroupsWithVariants(Guid menuItemId, Guid storeId);
    }
}

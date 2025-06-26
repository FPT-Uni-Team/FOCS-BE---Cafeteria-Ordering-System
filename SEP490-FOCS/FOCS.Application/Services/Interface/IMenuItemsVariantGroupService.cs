using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemsVariantGroupService
    {
        Task<List<MenuItemVariantGroup>> AssignMenuItemToVariantGroup(CreateMenuItemVariantGroupRequest request);

        Task<GetVariantGroupAndVariantResponse> GetVariantGroupsWithVariants(Guid menuItemId, Guid storeId);
    }
}

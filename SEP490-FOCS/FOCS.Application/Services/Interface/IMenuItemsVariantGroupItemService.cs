using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemsVariantGroupItemService
    {
        Task<bool> AssignMenuItemVariantGroupToMenuItemVariantItemGroup(CreateMenuItemVariantGroupItemRequest createMenuItemVariantGroupRequest);

        Task<bool> RemoveVariantsFromMenuItemVariantGroup(RemoveProductVariantFromProduct request, Guid menuItemId, string storeId);
    }
}

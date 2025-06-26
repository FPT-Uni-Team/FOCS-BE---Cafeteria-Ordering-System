using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IVariantGroupService
    {
        Task<List<VariantGroupDetailDTO>> GetVariantGroupsByMenuItemAsync(Guid menuItemId);

     
        Task<bool> AddMenuItemVariantToGroupAsync(AddVariantToGroupRequest request, Guid StoreId);

     
        Task<bool> RemoveVariantFromGroupAsync(Guid variantGroupId);

     
        Task<bool> UpdateGroupSettingsAsync(Guid menuItemId, string groupName, UpdateGroupSettingRequest request);

     
        Task<List<string>> GetGroupNamesByMenuItemAsync(Guid menuItemId);

        Task<bool> CreateVariantGroup(CreateVariantGroupRequest request, string storeId);
    }
}

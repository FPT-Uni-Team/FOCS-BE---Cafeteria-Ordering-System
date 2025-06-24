using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Common.Interfaces
{
    public interface IMenuItemVariantService
    {
        Task<MenuItemVariantDTO> CreateMenuItemVariant(MenuItemVariantDTO request, Guid StoreId);
        Task<PagedResult<MenuItemVariantDTO>> ListVariants(UrlQueryParameters urlQueryParameters, Guid storeId);
        Task<List<MenuItemVariantDTO>> ListVariantsWithIds(List<Guid> ids, Guid storeId);
        Task<bool> RemoveMenuItemVariant(Guid id, Guid storeId);
        Task<bool> UpdateMenuItemVariant(Guid Id, MenuItemVariantDTO request, Guid storeId);
        Task<MenuItemVariantDTO> GetVariantDetail(Guid id, Guid storeId);

        Task<bool> AssignVariantGroupToVariants(List<Guid> ids, Guid variantGroupId);
    }
}

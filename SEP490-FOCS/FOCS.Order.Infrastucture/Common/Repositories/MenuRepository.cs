using FOCS.Order.Infrastucture.Context;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Order.Infrastucture.Common.Repositories
{
    public class MenuRepository : Repository<MenuItem>
    {
        public MenuRepository(OrderDbContext context) : base(context)
        {
        }
        
        public async Task<List<MenuItemVariant>> GetMenuByStore(Guid storeId)
        {
            return _context.MenuItemVariants
                                .Include(iv => iv.VariantGroup)
                                .ThenInclude(vg => vg.MenuItem)
                                .ThenInclude(i => i.MenuCategory)
                                .Where(i => i.VariantGroup.MenuItem.MenuCategory.StoreId.Equals(storeId)
                                        && i.IsDeleted == false)
                                .ToList();
        }
    }
}

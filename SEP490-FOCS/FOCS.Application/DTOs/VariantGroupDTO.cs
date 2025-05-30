using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.DTOs
{
    public class VariantGroupDTO
    {
        public Guid id { get; set; }
        public string name { get; set; } 
        public MenuCategoryDTO MenuItem { get; set; }

        public ICollection<MenuItemVariant> Variants { get; set; }
    }
}

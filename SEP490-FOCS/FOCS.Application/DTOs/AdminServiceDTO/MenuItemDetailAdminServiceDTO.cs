using FOCS.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class MenuItemDetailAdminServiceDTO
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("images")]
        public string Images { get; set; }

        [JsonPropertyName("base_price")]
        public double BasePrice { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("menu_category_id")]
        public Guid MenuCategoryId { get; set; }

        [JsonPropertyName("menu_category")]
        public MenuCategoryDTO MenuCategory { get; set; }

        [JsonPropertyName("variant_groups")]
        public ICollection<VariantGroupDTO> VariantGroups { get; set; }
    }
}

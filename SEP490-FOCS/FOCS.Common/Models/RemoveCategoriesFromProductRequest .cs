using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models
{
    public class RemoveCategoriesFromProductRequest
    {
        [JsonPropertyName("menu_item_id")]
        public Guid MenuItemId { get; set; }

        [JsonPropertyName("cate_ids")]
        public List<Guid> CateIds { get; set; } = new();
    }

}

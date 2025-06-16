using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FOCS.Common.Models.CartModels
{
    public class UpdateCartItemQuantityRequest
    {
        [JsonPropertyName("cart_item_id")]
        public Guid CartItemId { get; set; }

        [JsonPropertyName("quantity")]
        public int NewQuantity { get; set; }
    }
}

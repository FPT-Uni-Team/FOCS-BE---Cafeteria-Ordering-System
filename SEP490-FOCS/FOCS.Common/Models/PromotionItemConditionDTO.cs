using FOCS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class PromotionItemConditionDTO
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [Required]
        [JsonPropertyName("buy_item_id")]
        public Guid BuyItemId { get; set; }

        [Required]
        [JsonPropertyName("buy_quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Buy Quantity must be greater than 0")]
        public int BuyQuantity { get; set; }

        [Required]
        [JsonPropertyName("get_item_id")]
        public Guid GetItemId { get; set; }

        [Required]
        [JsonPropertyName("get_quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Get Quantity must be greater than 0")]
        public int GetQuantity { get; set; }
    }
}

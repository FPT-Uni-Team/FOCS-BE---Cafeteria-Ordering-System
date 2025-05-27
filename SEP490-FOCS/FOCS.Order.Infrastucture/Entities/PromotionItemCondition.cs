using System;

namespace FOCS.Order.Infrastucture.Entities
{
    public class PromotionItemCondition
    {
        public Guid Id { get; set; }

        public Guid PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        
        public Guid BuyItemId { get; set; }
        public MenuItem BuyItem { get; set; }
        public int BuyQuantity { get; set; }

        
        public Guid GetItemId { get; set; }
        public MenuItem GetItem { get; set; }
        public int GetQuantity { get; set; }
    }
}

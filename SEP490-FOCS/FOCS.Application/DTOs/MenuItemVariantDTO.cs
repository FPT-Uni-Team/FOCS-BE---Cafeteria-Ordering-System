using FOCS.Common.Models;
using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.DTOs
{
    public class MenuItemVariantDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int PrepPerTime { get; set; }
        public int QuantityPerTime { get; set; }
        public bool IsAvailable { get; set; }

        public VariantGroup VariantGroup { get; set; }
    }
}

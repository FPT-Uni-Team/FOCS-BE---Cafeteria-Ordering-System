using FOCS.Order.Infrastucture.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.Application.DTOs
{
    public class MenuItemDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Images { get; set; }

        public double BasePrice { get; set; }

        public bool IsAvailable { get; set; }

        public MenuCategory MenuCategory { get; set; }

    }
}

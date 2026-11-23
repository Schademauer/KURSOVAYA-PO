using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.DAL
{
    public class OrderProduct
    {
        public int OrderItemId { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Связи
        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ICollection<OrderItemIngredient> OrderItemIngredients { get; set; } = new List<OrderItemIngredient>();
    }
}

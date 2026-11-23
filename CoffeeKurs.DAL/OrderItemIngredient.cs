using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.DAL
{
    public class OrderItemIngredient
    {
        public int OrderItemId { get; set; }
        public int IngredientId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }

        // Связи
        public OrderProduct? OrderProduct { get; set; }
        public Ingredient? Ingredient { get; set; }
    }
}
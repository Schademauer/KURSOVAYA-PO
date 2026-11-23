using System;
using System.Collections.Generic;
using System.Text;

namespace CoffeeKurs.DAL
{
    public class ProductIngredient
    {
        public int ProductId { get; set; }
        public int IngredientId { get; set; }
        public int DefaultQuantity { get; set; }

        // Связи для EF Core
        public Product? Product { get; set; }
        public Ingredient? Ingredient { get; set; }
    }
}
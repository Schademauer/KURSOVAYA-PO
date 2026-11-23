using System;
using System.Collections.Generic;
using System.Text;

using CoffeeKurs.DAL; // Ссылаемся на слой данных, чтобы использовать Product и Ingredient

namespace CoffeeKurs.BLL.Models
{
    public class SelectedIngredient
    {
        public Ingredient Ingredient { get; set; } = null!;
        public int Count { get; set; }
        public decimal Total => Ingredient.Price * Count;
    }

    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ProductDto Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public List<SelectedIngredient> Ingredients { get; set; } = new();

        public decimal UnitPrice => Product.Price + Ingredients.Sum(i => i.Total);
        public decimal TotalPrice => UnitPrice * Quantity;

        public string IngredientsText
        {
            get
            {
                if (Ingredients == null || !Ingredients.Any(i => i.Count > 0))
                    return "Классический";
                return string.Join(", ", Ingredients.Where(i => i.Count > 0)
                    .Select(i => $"{i.Ingredient.Name} ×{i.Count}"));
            }
        }
    }

    // DTO для клиента (чтобы не тащить пароль в интерфейс)
    public class ClientDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public int BonusPoints { get; set; }
    }
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
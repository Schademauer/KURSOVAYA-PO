using CoffeeKurs.BLL.Models;
using System.Collections.ObjectModel;

namespace CoffeeKurs.BLL.Services
{
    public class CartService
    {
        public ObservableCollection<CartItem> Cart { get; } = new();

        public void AddToCart(ProductDto product, List<SelectedIngredient>? ingredients = null) // Тут ProductDto!
        {
            Cart.Add(new CartItem
            {
                Product = product,
                Quantity = 1,
                Ingredients = ingredients ?? new List<SelectedIngredient>()
            });
        }

        public void RemoveFromCart(CartItem item) => Cart.Remove(item);
        public void ClearCart() => Cart.Clear();
        public decimal GetCartTotal() => Cart.Sum(c => c.TotalPrice);
    }
}
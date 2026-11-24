using CoffeeKurs.BLL.Models;
using System.Collections.ObjectModel;

namespace CoffeeKurs.BLL.Services
{
    public class CartService
    {
        public ObservableCollection<CartItem> Cart { get; } = new();

        public void AddToCart(ProductDto product, List<SelectedIngredient>? ingredients = null)
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

        public int GetMaxAvailableBonus(UserDto? user)
        {
            if (user == null) return 0;

            int total = (int)GetCartTotal();
            return Math.Min(user.BonusPoints, total);
        }
    }
}
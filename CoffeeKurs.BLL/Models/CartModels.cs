using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CoffeeKurs.DAL;

namespace CoffeeKurs.BLL.Models
{
    public class SelectedIngredient
    {
        public Ingredient Ingredient { get; set; } = null!;
        public int Count { get; set; }
        public decimal Total => Ingredient.Price * Count;
    }

    public class CartItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public Guid Id { get; set; } = Guid.NewGuid();
        public ProductDto Product { get; set; } = null!;

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(IngredientsText));
            }
        }

        public List<SelectedIngredient> Ingredients { get; set; } = new();

        public decimal UnitPrice => Product.Price + Ingredients.Sum(i => i.Total);
        public decimal TotalPrice => UnitPrice * Quantity;

        public string IngredientsText
        {
            get
            {
                if (Ingredients == null || !Ingredients.Any(i => i.Count > 0))
                    return "Классический";

                return string.Join(", ", Ingredients
                    .Where(i => i.Count > 0)
                    .Select(i => $"{i.Ingredient.Name} ×{i.Count}"));
            }
        }

        // Вызывать после изменения списка ингредиентов
        public void NotifyIngredientsChanged()
        {
            OnPropertyChanged(nameof(UnitPrice));
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(IngredientsText));
        }
    }

    public class ProductDto : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;

        private bool _canBuy = true;
        public bool CanBuy
        {
            get => _canBuy;
            set
            {
                if (_canBuy == value) return;
                _canBuy = value;
                OnPropertyChanged();
            }
        }
    }
}
using CoffeeKurs.BLL.Models;
using CoffeeKurs.BLL.Services;
using CoffeeKurs.PL.Base;
using CoffeeKurs.PL.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CoffeeKurs.PL.ViewModels
{
    public class IngredientsViewModel : ViewModelBase
    {
        private readonly ProductService _productService;
        private readonly CartItem _cartItem;

        public string TitleText { get; }
        public ObservableCollection<IngredientItem> Ingredients { get; } = new();

        public RelayCommand IncreaseCommand { get; }
        public RelayCommand DecreaseCommand { get; }
        public RelayCommand SaveCommand { get; }

        public IngredientsViewModel(CartItem cartItem, ProductService productService)
        {
            _cartItem = cartItem;
            _productService = productService;
            TitleText = cartItem.Product.Name;

            // 1. Получаем ингредиенты из БД
            var dbIngredients = _productService.GetIngredientsForProduct(cartItem.Product.ProductId);

            // 2. Заполняем коллекцию
            foreach (var dbItem in dbIngredients)
            {
                // Ищем, есть ли уже этот ингредиент в корзине
                var existing = cartItem.Ingredients.FirstOrDefault(x => x.Ingredient?.IngredientId == dbItem.IngredientId);

                Ingredients.Add(new IngredientItem
                {
                    Id = dbItem.IngredientId,
                    IngredientName = dbItem.Ingredient?.Name ?? "Неизвестно",
                    Price = dbItem.Ingredient?.Price ?? 0,
                    Count = existing != null ? existing.Count : 0
                });
            }

            IncreaseCommand = new RelayCommand(Increase);
            DecreaseCommand = new RelayCommand(Decrease);
            SaveCommand = new RelayCommand(_ => Save());
        }

        private void Increase(object? param)
        {
            if (param is IngredientItem item) item.Count++;
        }

        private void Decrease(object? param)
        {
            if (param is IngredientItem item && item.Count > 0) item.Count--;
        }

        private void Save()
        {
            _cartItem.Ingredients.Clear();

            var allDbIngredients = _productService.GetIngredientsForProduct(_cartItem.Product.ProductId);

            foreach (var item in Ingredients.Where(x => x.Count > 0))
            {
                var dbIngredient = allDbIngredients.FirstOrDefault(x => x.IngredientId == item.Id);
                if (dbIngredient?.Ingredient != null)
                {
                    _cartItem.Ingredients.Add(new SelectedIngredient
                    {
                        Ingredient = dbIngredient.Ingredient,
                        Count = item.Count
                    });
                }
            }

            _cartItem.NotifyIngredientsChanged();   // ← важно

            var window = Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }

    public class IngredientItem : ViewModelBase
    {
        public int Id { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string PriceText => $"{Price:0} ₽ / ед.";

        private int _count;
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
    }
}
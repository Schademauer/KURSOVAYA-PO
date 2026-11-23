using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using CoffeeKurs.BLL.Services;  // Подключаем BLL
using CoffeeKurs.BLL.Models;    // Подключаем DTO
using CoffeeKurs.DAL;           // Ссылаемся на DAL для типов Product и Ingredient

namespace CoffeeKurs.PL
{
    public partial class MainWindow : Window
    {
        private readonly OrderService _orderService = new();
        // Создаем экземпляры наших сервисов
        private readonly ProductService _productService = new();
        private readonly AuthService _authService = new();
        private UserDto? _currentUser = null; // Текущий пользователь
        private readonly CartService _cartService = new();

        private CartItem currentEditingItem = null;
        private Dictionary<int, int> tempCounts = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            UpdateUserInfo();
        }

        private void LoadProducts()
        {
            try
            {
                // Используем сервис из BLL
                ProductsList.ItemsSource = _productService.GetAllAvailableProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров:\n" + ex.Message, "Ошибка БД",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ... (Метод UpdateUserInfo оставляем без изменений, он работает с UI)

        private void UpdateUserInfo()
        {
            if (_currentUser != null)
            {
                UserInfoText.Text = _currentUser.DisplayName; // Выводим имя
                LoginButton.Content = "Выйти";
            }
            else
            {
                UserInfoText.Text = "Гость";
                LoginButton.Content = "Войти";
            }
        LoginButton.Content = "Войти";
        }
        // ==================== НАВИГАЦИЯ ====================

        private void ShowView(UIElement view)
        {
            LoginView.Visibility = Visibility.Collapsed;
            MenuView.Visibility = Visibility.Collapsed;
            CartView.Visibility = Visibility.Collapsed;
            IngredientsView.Visibility = Visibility.Collapsed;
            PaymentView.Visibility = Visibility.Collapsed;
            AdminView.Visibility = Visibility.Collapsed;

            view.Visibility = Visibility.Visible;
        }

        private void ShowMenu_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            ShowView(MenuView);
        }

        private void ShowCart_Click(object sender, RoutedEventArgs e)
        {
            RefreshCart();
            ShowView(CartView);
        }

        private void ShowPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_cartService.Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            PaymentItemsList.ItemsSource = null;
            PaymentItemsList.ItemsSource = _cartService.Cart.ToList();
            PaymentTotalText.Text = $"{_cartService.GetCartTotal():0} ₽";
            ShowView(PaymentView);
        }

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser != null)
            {
                // Если пользователь авторизован — выходим
                _currentUser = null;
                UpdateUserInfo();
                ShowView(MenuView);
                MessageBox.Show("Вы вышли из аккаунта.");
            }
            else
            {
                // Если нет — показываем окно входа
                ShowView(LoginView);
            }
        }

        // ==================== КОРЗИНА ====================

        private void RefreshCart()
        {
            CartList.ItemsSource = null;
            CartList.ItemsSource = _cartService.Cart.ToList();
            CartTotalText.Text = $"Итого: {_cartService.GetCartTotal():0} ₽";
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductDto product)
            {
                _cartService.AddToCart(product); // Используем сервис
                btn.Content = "В корзине";
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A66B4A"));
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                _cartService.RemoveFromCart(item);
                RefreshCart();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                item.Quantity++;
                RefreshCart();
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                if (item.Quantity > 1)
                    item.Quantity--;
                else
                    _cartService.RemoveFromCart(item);
                RefreshCart();
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            _cartService.ClearCart();
            ShowView(MenuView);
            LoadProducts();
        }

        // ==================== ИНГРЕДИЕНТЫ ====================

        private void EditIngredients_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItem item)
            {
                currentEditingItem = item;
                tempCounts.Clear();

                foreach (var si in item.Ingredients)
                    tempCounts[si.Ingredient.IngredientId] = si.Count;

                // Получаем ингредиенты через BLL
                var ingredients = _productService.GetIngredientsForProduct(item.Product.ProductId);
                IngredientsList.ItemsSource = ingredients;
                IngredientsTitle.Text = item.Product.Name;
                ShowView(IngredientsView);
            }
        }
        // ==================== ОПЛАТА (Заглушка) ====================
        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (_cartService.Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Передаем корзину, комментарий и текущего пользователя
                int orderId = _orderService.CreateOrder(
                    _cartService.Cart.ToList(), 
                    CommentBox.Text, 
                    _currentUser
                );

                MessageBox.Show($"Заказ №{orderId} успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Очищаем корзину и возвращаемся в меню
                _cartService.ClearCart();
                CommentBox.Text = "";
                ShowView(MenuView);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа:\n" + ex.Message, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== АВТОРИЗАЦИЯ ====================
        private void DoLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.");
                return;
            }

            try
            {
                var user = _authService.Login(login, password);

                if (user != null)
                {
                    // 1. СНАЧАЛА присваиваем пользователя
                    _currentUser = user;

                    // 2. ПОТОМ обновляем интерфейс (и кнопка поменяется!)
                    UpdateUserInfo();
                    LoginButton.Content = "Выйти";
                    MessageBox.Show($"Добро пожаловать, {user.FirstName}!");

                    LoginBox.Text = "";
                    PasswordBox.Password = "";

                    if (user.IsAdmin)
                        ShowView(AdminView);
                    else
                        ShowView(MenuView);
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при авторизации:\n" + ex.Message);
            }
        }
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Логика отчета будет добавлена на следующем этапе!", "В разработке");
        }
        // ==================== ИНГРЕДИЕНТЫ (Логика UI) ====================

        private void IngredientPlus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductIngredient pi)
            {
                if (!tempCounts.ContainsKey(pi.IngredientId))
                    tempCounts[pi.IngredientId] = 0;

                tempCounts[pi.IngredientId]++;
                UpdateCountText(btn, tempCounts[pi.IngredientId]);
            }
        }

        private void IngredientMinus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductIngredient pi)
            {
                if (!tempCounts.ContainsKey(pi.IngredientId)) return;
                if (tempCounts[pi.IngredientId] > 0)
                    tempCounts[pi.IngredientId]--;
                UpdateCountText(btn, tempCounts[pi.IngredientId]);
            }
        }

        private void UpdateCountText(Button btn, int count)
        {
            if (btn.Parent is StackPanel sp)
            {
                var tb = sp.Children.OfType<TextBlock>().FirstOrDefault();
                if (tb != null) tb.Text = count.ToString();
            }
        }

        private void SaveIngredients_Click(object sender, RoutedEventArgs e)
        {
            if (currentEditingItem == null) return;

            currentEditingItem.Ingredients.Clear();
            var all = _productService.GetIngredientsForProduct(currentEditingItem.Product.ProductId);

            foreach (var kv in tempCounts.Where(x => x.Value > 0))
            {
                var pi = all.FirstOrDefault(x => x.IngredientId == kv.Key);
                if (pi != null)
                {
                    currentEditingItem.Ingredients.Add(new SelectedIngredient
                    {
                        Ingredient = pi.Ingredient, // Тут используется Ingredient из DAL, 
                                                    // так как мы его подтягиваем через BLL
                        Count = kv.Value
                    });
                }
            }

            currentEditingItem = null;
            RefreshCart();
            ShowView(CartView);
        }
        // Остальные методы (IngredientPlus_Click, IngredientMinus_Click, UpdateCountText, SaveIngredients_Click) оставляем без изменений, они работают с UI
    }
}
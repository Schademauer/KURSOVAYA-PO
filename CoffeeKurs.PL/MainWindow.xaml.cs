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

                ProductsList.ItemsSource = _productService.GetAllAvailableProducts();//для отображения меню 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки товаров:\n" + ex.Message, "Ошибка БД",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void UpdateUserInfo() //Для отображения в шапке пользощвателя, его имени и кнопок
        {
            if (_currentUser != null)
            {
                UserInfoText.Text = _currentUser.DisplayName;
                LoginButton.Content = "Выйти";
            }
            else
            {
                UserInfoText.Text = "Гость";
                LoginButton.Content = "Войти";
            }
        LoginButton.Content = "Войти";
        }
        //НАВИГАЦИЯ 

        /// <summary>
        /// Функция для показа представлений
        /// 
        /// </summary>
        /// <param name="view"></param>
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

        /// <summary>
        /// Функция для кнопки в шапке для входа пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowLogin_Click(object sender, RoutedEventArgs e)//кнопка в шапке для входа пользователя
        {
            if (_currentUser != null)
            {
                // авторизован? Да ВЫХОД
                _currentUser = null;
                UpdateUserInfo();
                ShowView(MenuView);
                MessageBox.Show("Вы вышли из аккаунта.");
            }
            else
            {
                //НЕТ _ форма входа
                ShowView(LoginView);
            }
        }

        // КОРЗИНА 

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
                _cartService.AddToCart(product);
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

        //  ИНГРЕДИЕНТЫ 

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
        //  ОПЛАТА 
        private void ShowPayment_Click(object sender, RoutedEventArgs e)
        {
            if (_cartService.Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста");
                return;
            }

            PaymentItemsList.ItemsSource = null;
            PaymentItemsList.ItemsSource = _cartService.Cart.ToList();

            decimal total = _cartService.GetCartTotal();
            PaymentTotalText.Text = $"{total:0} ₽";
            FinalPaymentText.Text = $"{total:0} ₽";

            if (_currentUser != null && _currentUser.BonusPoints > 0)
            {
                BonusPanel.Visibility = Visibility.Visible;
                int maxBonus = _cartService.GetMaxAvailableBonus(_currentUser);
                MaxBonusText.Text = $"(Доступно: {maxBonus})";
                UseBonusCheckBox.IsChecked = false;
                BonusInputGrid.Visibility = Visibility.Collapsed;
                BonusInput.Text = "0";
            }
            else
            {
                BonusPanel.Visibility = Visibility.Collapsed;
            }

            ShowView(PaymentView);
        }

        private void UseBonusCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BonusInputGrid.Visibility = Visibility.Visible;
            BonusInput.Text = _cartService.GetMaxAvailableBonus(_currentUser).ToString();
        }

        private void UseBonusCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            BonusInputGrid.Visibility = Visibility.Collapsed;
            BonusInput.Text = "0";
            UpdateFinalPaymentText();
        }

        private void BonusInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFinalPaymentText();
        }

        private void UpdateFinalPaymentText()
        {
            decimal total = _cartService.GetCartTotal();
            int bonusToUse = 0;

            if (UseBonusCheckBox.IsChecked == true && int.TryParse(BonusInput.Text, out int inputBonus))
            {
                int maxBonus = _cartService.GetMaxAvailableBonus(_currentUser);
                bonusToUse = Math.Min(inputBonus, maxBonus);
                bonusToUse = Math.Min(bonusToUse, (int)total);
            }

            decimal final = total - bonusToUse;
            FinalPaymentText.Text = $"{final:0} ₽";
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (_cartService.Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int bonusToUse = 0;
            if (UseBonusCheckBox.IsChecked == true && int.TryParse(BonusInput.Text, out int inputBonus))
            {
                bonusToUse = Math.Min(inputBonus, _cartService.GetMaxAvailableBonus(_currentUser));
            }

            try
            {
                int orderId = _orderService.CreateOrder(
                    _cartService.Cart.ToList(),
                    CommentBox.Text,
                    _currentUser,
                    bonusToUse
                );

                if (_currentUser != null)
                {
                    _currentUser = _authService.GetUserById(_currentUser.UserId);
                    UpdateUserInfo();
                }

                MessageBox.Show($"Заказ №{orderId} успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                _cartService.ClearCart();
                CommentBox.Text = "";
                ShowView(MenuView);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ошибка при оформлении заказа:\n" + errorMsg, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //  АВТОРИЗАЦИЯ 
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
                    
                    _currentUser = user;

                    
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
        // ИНГРЕДИЕНТЫ 

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
                        Ingredient = pi.Ingredient,  
                                                    
                        Count = kv.Value
                    });
                }
            }

            currentEditingItem = null;
            RefreshCart();
            ShowView(CartView);
        }
    }
}
using CoffeeKurs.BLL.Models;
using CoffeeKurs.BLL.Services;
using CoffeeKurs.PL.Base;
using CoffeeKurs.PL.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CoffeeKurs.PL.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ProductService _productService = new();
        private readonly OrderService _orderService = new();
        private readonly AuthService _authService = new();
        private readonly CartService _cartService = new();

        // Делегат для открытия окна ингредиентов (вызывается из MainWindow.xaml.cs)
        public Action<CartItem>? OpenIngredientsWindow { get; set; }

        // --- Управление экранами (Навигация) ---
        private string _currentView = "Menu"; // СТАРТОВЫЙ ЭКРАН - МЕНЮ
        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // --- Данные для Меню ---
        public ObservableCollection<ProductDto> Products { get; } = new();

        // --- Данные для Корзины ---
        public ObservableCollection<CartItem> Cart => _cartService.Cart;

        public string CartTotalText => $"Итого: {_cartService.GetCartTotal():0} ₽";
        public string PaymentTotalText => $"{_cartService.GetCartTotal():0} ₽";
        public string FinalPaymentText => $"{_cartService.GetCartTotal():0} ₽";

        private string _commentInput = string.Empty;
        public string CommentInput
        {
            get => _commentInput;
            set => SetProperty(ref _commentInput, value);
        }

        // --- Данные для Авторизации ---
        private string _loginInput = string.Empty;
        public string LoginInput
        {
            get => _loginInput;
            set => SetProperty(ref _loginInput, value);
        }

        private string _passwordInput = string.Empty;
        public string PasswordInput
        {
            get => _passwordInput;
            set => SetProperty(ref _passwordInput, value);
        }

        private UserDto? _currentUser;
        public UserDto? CurrentUser
        {
            get => _currentUser;
            set
            {
                SetProperty(ref _currentUser, value);
                OnPropertyChanged(nameof(UserDisplayName));
                OnPropertyChanged(nameof(LoginButtonText));
            }
        }

        public string UserDisplayName => CurrentUser != null ? CurrentUser.DisplayName : "Гость";
        public string LoginButtonText => CurrentUser != null ? "Выйти" : "Войти";

        // --- Команды ---
        public RelayCommand ShowLoginViewCommand { get; }
        public RelayCommand ShowMenuViewCommand { get; }
        public RelayCommand ShowCartViewCommand { get; }
        public RelayCommand ShowPaymentViewCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public RelayCommand LoginCommand { get; }
        public RelayCommand AddToCartCommand { get; }
        public RelayCommand RemoveFromCartCommand { get; }
        public RelayCommand IncreaseQuantityCommand { get; }
        public RelayCommand DecreaseQuantityCommand { get; }
        public RelayCommand EditIngredientsCommand { get; }
        public RelayCommand PayCommand { get; }

        public MainWindowViewModel()
        {
            // 1. Сразу загружаем товары при запуске (для гостя)
            LoadProducts();

            // 2. Настраиваем команды навигации
            ShowLoginViewCommand = new RelayCommand(_ => CurrentView = "Login");
            ShowMenuViewCommand = new RelayCommand(_ => { LoadProducts(); CurrentView = "Menu"; });
            ShowCartViewCommand = new RelayCommand(_ => CurrentView = "Cart");
            ShowPaymentViewCommand = new RelayCommand(_ => CurrentView = "Payment");

            LogoutCommand = new RelayCommand(_ =>
            {
                CurrentUser = null;
                LoadProducts();
                CurrentView = "Menu";
                MessageBox.Show("Вы вышли из аккаунта.");
            });

            // 3. Логин
            LoginCommand = new RelayCommand(_ => DoLogin());

            // 4. Действия с корзиной
            AddToCartCommand = new RelayCommand(param => AddToCart(param as ProductDto));
            RemoveFromCartCommand = new RelayCommand(param => { _cartService.RemoveFromCart(param as CartItem); RefreshCart(); });
            IncreaseQuantityCommand = new RelayCommand(param => { if (param is CartItem item) { item.Quantity++; RefreshCart(); } });
            DecreaseQuantityCommand = new RelayCommand(param => { if (param is CartItem item) { if (item.Quantity > 1) item.Quantity--; else _cartService.RemoveFromCart(item); RefreshCart(); } });
            IncreaseQuantityCommand = new RelayCommand(param =>
            {
                if (param is CartItem item)
                {
                    item.Quantity++;
                    RefreshCart();
                }
            });

            DecreaseQuantityCommand = new RelayCommand(param =>
            {
                if (param is CartItem item)
                {
                    if (item.Quantity > 1)
                        item.Quantity--;
                    else
                        _cartService.RemoveFromCart(item);

                    RefreshCart();
                }
            });
            // 5. Ингредиенты
            EditIngredientsCommand = new RelayCommand(param => { if (param is CartItem item) OpenIngredientsWindow?.Invoke(item); });

            // 6. Оплата
            PayCommand = new RelayCommand(_ => Pay());
        }

        // --- Методы ---
        public void RefreshUIAfterIngredientsChanged()
        {
            // Уведомляем каждый элемент корзины, что ингредиенты изменились
            foreach (var item in Cart)
                item.NotifyIngredientsChanged();

            RefreshCart();
            CurrentView = "Cart";
        }
        public void LoadProducts()
        {
            try
            {
                Products.Clear();
                bool isGuest = CurrentUser == null;
                foreach (var p in _productService.GetAllAvailableProducts(isGuest))
                    Products.Add(p);
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки товаров: " + ex.Message); }
        }

        private void AddToCart(ProductDto? product)
        {
            if (product == null) return;

            _cartService.AddToCart(product);
            product.CanBuy = false;          // теперь сработает уведомление
            RefreshCart();
        }
        // Метод, который вызывается после закрытия окна ингредиентов
        private void DoLogin()
        {
            if (string.IsNullOrWhiteSpace(LoginInput) || string.IsNullOrWhiteSpace(PasswordInput))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.");
                return;
            }
            var user = _authService.Login(LoginInput, PasswordInput);
            if (user != null)
            {
                CurrentUser = user;
                LoginInput = "";
                PasswordInput = "";
                MessageBox.Show($"Добро пожаловать, {user.FirstName}!");
                LoadProducts();
                CurrentView = user.IsAdmin ? "Admin" : "Menu";
            }
            else MessageBox.Show("Неверный логин или пароль.");
        }

        private void Pay()
        {
            if (Cart.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                int orderId = _orderService.CreateOrder(Cart.ToList(), CommentInput, CurrentUser, 0);
                MessageBox.Show($"Заказ №{orderId} успешно оформлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                _cartService.ClearCart();
                CommentInput = "";
                RefreshCart();
                LoadProducts(); // Сбрасываем кнопки "В корзине"
                CurrentView = "Menu";
            }
            catch (Exception ex)
            {
                string errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show("Ошибка при оформлении заказа:\n" + errorMsg, "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshCart()
        {
            OnPropertyChanged(nameof(CartTotalText));
            OnPropertyChanged(nameof(PaymentTotalText));
            OnPropertyChanged(nameof(FinalPaymentText));
        }

    
    }
}
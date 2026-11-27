using CoffeeKurs.BLL.Services;
using CoffeeKurs.PL.ViewModels;
using System.Windows;

namespace CoffeeKurs.PL
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (DataContext is MainWindowViewModel vm)
            {
                vm.OpenIngredientsWindow = cartItem =>
                {
                    var ingredientsVm = new IngredientsViewModel(cartItem, new ProductService());

                    var window = new IngredientsWindow
                    {
                        DataContext = ingredientsVm,
                        Owner = this
                    };

                    window.ShowDialog();

                    // После закрытия окна обновляем UI
                    vm.RefreshUIAfterIngredientsChanged();
                };
            }
        }
    }
}
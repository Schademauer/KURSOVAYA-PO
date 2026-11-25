using CoffeeKurs.BLL.Models;
using CoffeeKurs.DAL;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class ProductService
    {
        //для отображения меню
        public List<ProductDto> GetAllAvailableProducts(bool isGuest)
        {
            using var context = new AppDbContext();

            return context.Products
                .Where(p => p.IsAvailable == true)
                .OrderBy(p => p.ProductId)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    CanBuy = !isGuest
                })
                .ToList();
        }

        public List<ProductIngredient> GetIngredientsForProduct(int productId)
        {
            using var context = new AppDbContext();

            return context.ProductIngredients
                .Include(pi => pi.Ingredient)
                .Where(pi => pi.ProductId == productId && pi.Ingredient!.IsAvailable == true)
                .OrderBy(pi => pi.Ingredient!.Name) 
                .ToList();
        }
    }
}
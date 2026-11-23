using CoffeeKurs.BLL.Models;
using CoffeeKurs.DAL;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class ProductService
    {
        public List<ProductDto> GetAllAvailableProducts()
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
                    Description = p.Description
                })
                .ToList();
        }

        public List<ProductIngredient> GetIngredientsForProduct(int productId)
        {
            using var context = new AppDbContext();

            return context.ProductIngredients
                .Include(pi => pi.Ingredient) // Жадная загрузка: подтягиваем данные ингредиента
                .Where(pi => pi.ProductId == productId && pi.Ingredient!.IsAvailable == true)
                //.OrderBy(pi => pi.IsOptional) // <--- УДАЛИ ЭТУ СТРОКУ (или закомментируй)
                .OrderBy(pi => pi.Ingredient!.Name) // Сортировка по имени
                .ToList();
        }
    }
}
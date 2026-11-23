using CoffeeKurs.DAL;
using CoffeeKurs.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class OrderService
    {
        // Метод принимает корзину, комментарий и текущего пользователя (если он есть)
        public int CreateOrder(List<CartItem> cart, string comment, UserDto? currentUser)
        {
            if (cart == null || cart.Count == 0)
                throw new Exception("Корзина пуста.");

            using var context = new AppDbContext();

            // Начинаем транзакцию (чтобы всё сохранилось разом или не сохранилось вообще)
            using var transaction = context.Database.BeginTransaction();

            try
            {
                // 1. Создаем сам заказ (Orders)
                var totalAmount = cart.Sum(c => c.TotalPrice);
                var order = new Order
                {
                    ClientId = currentUser?.UserId, // Если гость - null
                    StatusId = 1, // 1 - "created" (создан)
                    TotalAmount = totalAmount,
                    Comment = comment,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    CreatedTime = TimeOnly.FromDateTime(DateTime.Now),
                    FinalAmount = totalAmount, // Пока без списания бонусов
                    BonusPointsUsed = 0,
                    BonusPointsEarned = 0
                };

                context.Orders.Add(order);
                context.SaveChanges(); // Сохраняем, чтобы получить OrderId

                // 2. Добавляем позиции заказа (Order_products)
                foreach (var item in cart)
                {
                    var orderProduct = new OrderProduct
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Product.ProductId,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice // Цена на момент заказа
                    };

                    context.OrderProducts.Add(orderProduct);
                    context.SaveChanges(); // Сохраняем, чтобы получить OrderItemId

                    // 3. Добавляем кастомизацию (ингредиенты) для этой позиции
                    foreach (var ing in item.Ingredients)
                    {
                        var orderItemIngredient = new OrderItemIngredient
                        {
                            OrderItemId = orderProduct.OrderItemId,
                            IngredientId = ing.Ingredient.IngredientId,
                            Quantity = ing.Count,
                            PriceAtPurchase = ing.Ingredient.Price
                        };
                        context.OrderItemIngredients.Add(orderItemIngredient);
                    }
                }

                // Начисляем бонусы (например, 5% от суммы) если пользователь авторизован
                if (currentUser != null)
                {
                    var user = context.Users.Find(currentUser.UserId);
                    if (user != null)
                    {
                        int earned = (int)(totalAmount * 0.05m); // 5% бонусов
                        user.BonusPoints += earned;
                        order.BonusPointsEarned = earned;
                        context.SaveChanges();
                    }
                }

                transaction.Commit(); // Фиксируем все изменения
                return order.OrderId; // Возвращаем номер заказа
            }
            catch (Exception)
            {
                transaction.Rollback(); // Если ошибка - откатываем всё
                throw;
            }
        }
    }
}
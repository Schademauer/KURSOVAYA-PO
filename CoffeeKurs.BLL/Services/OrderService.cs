using CoffeeKurs.DAL;
using CoffeeKurs.BLL.Models;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.BLL.Services
{
    public class OrderService
    {
        public int CreateOrder(List<CartItem> cart, string comment, UserDto? currentUser, int bonusPointsToUse)
        {
            if (cart == null || cart.Count == 0)
                throw new Exception("Корзина пуста.");

            using var context = new AppDbContext();
            using var transaction = context.Database.BeginTransaction();

            try
            {
                var totalAmount = cart.Sum(c => c.TotalPrice);
                int actualBonusUsed = 0;
                User? dbUser = null;

                if (currentUser != null && bonusPointsToUse > 0)
                {
                    dbUser = context.Users.Find(currentUser.UserId);
                    if (dbUser != null && dbUser.BonusPoints >= bonusPointsToUse)
                    {
                        actualBonusUsed = bonusPointsToUse;
                        dbUser.BonusPoints -= actualBonusUsed;
                    }
                }

                var finalAmount = totalAmount - actualBonusUsed;
                if (finalAmount < 0) finalAmount = 0;

                var order = new Order
                {
                    ClientId = currentUser?.UserId,
                    StatusId = 1,
                    TotalAmount = totalAmount,
                    Comment = comment,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now),
                    CreatedTime = TimeOnly.FromDateTime(DateTime.Now),
                    FinalAmount = finalAmount,
                    BonusPointsUsed = actualBonusUsed,
                    BonusPointsEarned = 0
                };

                context.Orders.Add(order);
                context.SaveChanges();

                foreach (var item in cart)
                {
                    var orderProduct = new OrderProduct
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Product.ProductId,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice
                    };

                    context.OrderProducts.Add(orderProduct);
                    context.SaveChanges();

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

                if (currentUser != null && dbUser != null)
                {
                    int earned = (int)(finalAmount * 0.10m);
                    dbUser.BonusPoints += earned;
                    order.BonusPointsEarned = earned;
                    context.SaveChanges();
                }

                transaction.Commit();
                return order.OrderId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
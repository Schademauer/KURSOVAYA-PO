using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CoffeeKurs.DAL
{
    public class AppDbContext : DbContext
    {
        // Убедись, что пароль правильный!
        private const string ConnectionString = "Host=localhost;Port=5432;Database=testkurs2;Username=postgres;Password=password;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        // Все таблицы БД
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<OrderItemIngredient> OrderItemIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ================= ПРОДУКТЫ =================
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<Product>().HasKey(p => p.ProductId); // <--- ДОБАВЛЕНО
            modelBuilder.Entity<Product>().Property(p => p.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<Product>().Property(p => p.CategoryId).HasColumnName("category_id");
            modelBuilder.Entity<Product>().Property(p => p.IsAvailable).HasColumnName("is_available");
            modelBuilder.Entity<Product>().Property(p => p.Name).HasColumnName("name");
            modelBuilder.Entity<Product>().Property(p => p.Description).HasColumnName("description");
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnName("price");
            // ================= ИНГРЕДИЕНТЫ =================
            modelBuilder.Entity<Ingredient>().ToTable("ingredients");
            modelBuilder.Entity<Ingredient>().HasKey(i => i.IngredientId); // <--- ДОБАВЛЕНО
            modelBuilder.Entity<Ingredient>().Property(i => i.IngredientId).HasColumnName("ingredient_id");
            modelBuilder.Entity<Ingredient>().Property(i => i.IsAvailable).HasColumnName("is_available");
            modelBuilder.Entity<Ingredient>().Property(i => i.Name).HasColumnName("name");
            modelBuilder.Entity<Ingredient>().Property(i => i.Unit).HasColumnName("unit");
            modelBuilder.Entity<Ingredient>().Property(i => i.Price).HasColumnName("price");

            // ================= СВЯЗЬ ПРОДУКТ-ИНГРЕДИЕНТ =================
            modelBuilder.Entity<ProductIngredient>().ToTable("product_ingredients");
            modelBuilder.Entity<ProductIngredient>().HasKey(pi => new { pi.ProductId, pi.IngredientId }); // Составной ключ
            modelBuilder.Entity<ProductIngredient>().Property(pi => pi.ProductId).HasColumnName("product_id");
            modelBuilder.Entity<ProductIngredient>().Property(pi => pi.IngredientId).HasColumnName("ingredient_id");
            modelBuilder.Entity<ProductIngredient>().Property(pi => pi.DefaultQuantity).HasColumnName("quantity");

            // ================= ПОЛЬЗОВАТЕЛИ =================
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<User>().HasKey(u => u.UserId); // <--- ДОБАВЛЕНО
            modelBuilder.Entity<User>().Property(u => u.UserId).HasColumnName("user_id");
            modelBuilder.Entity<User>().Property(u => u.UserTypeId).HasColumnName("user_type_id");
            modelBuilder.Entity<User>().Property(u => u.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<User>().Property(u => u.LastName).HasColumnName("last_name");
            modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasColumnName("password_hash");
            modelBuilder.Entity<User>().Property(u => u.IsActive).HasColumnName("is_active");
            modelBuilder.Entity<User>().Property(u => u.BonusPoints).HasColumnName("bonus_points");
            modelBuilder.Entity<User>().Property(u => u.BonusPointsActive).HasColumnName("bonus_points_active");
            modelBuilder.Entity<User>().Property(u => u.RegistrationDate).HasColumnName("registration_date");

            // ================= ЗАКАЗЫ =================
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<Order>().HasKey(o => o.OrderId); // <--- ДОБАВЛЕНО (исправляет ошибку)
            modelBuilder.Entity<Order>().Property(o => o.OrderId).HasColumnName("order_id");
            modelBuilder.Entity<Order>().Property(o => o.ClientId).HasColumnName("client_id");
            modelBuilder.Entity<Order>().Property(o => o.StatusId).HasColumnName("status_id");
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnName("total_amount");
            modelBuilder.Entity<Order>().Property(o => o.CreatedTime).HasColumnName("created_time");
            modelBuilder.Entity<Order>().Property(o => o.CreatedDate).HasColumnName("created_date");
            modelBuilder.Entity<Order>().Property(o => o.BonusPointsUsed).HasColumnName("bonus_points_used");
            modelBuilder.Entity<Order>().Property(o => o.BonusPointsEarned).HasColumnName("bonus_points_earned");
            modelBuilder.Entity<Order>().Property(o => o.FinalAmount).HasColumnName("final_amount");

            // ================= ПОЗИЦИИ В ЗАКАЗАХ =================
            modelBuilder.Entity<OrderProduct>().ToTable("order_products");
            modelBuilder.Entity<OrderProduct>().HasKey(op => op.OrderItemId); // <--- ДОБАВЛЕНО (исправляет ошибку)
            modelBuilder.Entity<OrderProduct>().Property(op => op.OrderItemId).HasColumnName("order_item_id");
            modelBuilder.Entity<OrderProduct>().Property(op => op.OrderId).HasColumnName("order_id");
            modelBuilder.Entity<OrderProduct>().Property(op => op.ProductId).HasColumnName("product_id");

            // ================= ИНГРЕДИЕНТЫ В ЗАКАЗАХ =================
            modelBuilder.Entity<OrderItemIngredient>().ToTable("order_item_ingredients");
            modelBuilder.Entity<OrderItemIngredient>().HasKey(oii => new { oii.OrderItemId, oii.IngredientId }); // Составной ключ
            modelBuilder.Entity<OrderItemIngredient>().Property(oii => oii.OrderItemId).HasColumnName("order_item_id");
            modelBuilder.Entity<OrderItemIngredient>().Property(oii => oii.IngredientId).HasColumnName("ingredient_id");
            modelBuilder.Entity<OrderItemIngredient>().Property(oii => oii.PriceAtPurchase).HasColumnName("price_at_purchase");
        }
    }
}
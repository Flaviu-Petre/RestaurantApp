using Microsoft.EntityFrameworkCore;
using RestaurantApp.Core.Models;
using System;

namespace RestaurantApp.Data
{
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        // DbSet properties for each entity
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishImage> DishImages { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuDish> MenuDishes { get; set; }
        public DbSet<Allergen> Allergens { get; set; }
        public DbSet<DishAllergen> DishAllergens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure Dish entity
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.ToTable("Dishes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PortionQuantity).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalQuantity).IsRequired().HasColumnType("decimal(18,2)");

                // Configure one-to-many relationship with Category
                entity.HasOne(d => d.Category)
                      .WithMany(c => c.Dishes)
                      .HasForeignKey(d => d.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DishImage entity
            modelBuilder.Entity<DishImage>(entity =>
            {
                entity.ToTable("DishImages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(500);

                // Configure one-to-many relationship with Dish
                entity.HasOne(di => di.Dish)
                      .WithMany(d => d.Images)
                      .HasForeignKey(di => di.DishId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Allergen entity
            modelBuilder.Entity<Allergen>(entity =>
            {
                entity.ToTable("Allergens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // Configure DishAllergen entity (many-to-many relationship)
            modelBuilder.Entity<DishAllergen>(entity =>
            {
                entity.ToTable("DishAllergens");
                entity.HasKey(e => new { e.DishId, e.AllergenId });

                // Configure many-to-many relationship
                entity.HasOne(da => da.Dish)
                      .WithMany(d => d.DishAllergens)
                      .HasForeignKey(da => da.DishId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(da => da.Allergen)
                      .WithMany(a => a.DishAllergens)
                      .HasForeignKey(da => da.AllergenId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Menu entity
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menus");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);

                // Configure one-to-many relationship with Category
                entity.HasOne(m => m.Category)
                      .WithMany(c => c.Menus)
                      .HasForeignKey(m => m.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MenuDish entity (many-to-many relationship with custom properties)
            modelBuilder.Entity<MenuDish>(entity =>
            {
                entity.ToTable("MenuDishes");
                entity.HasKey(e => new { e.MenuId, e.DishId });
                entity.Property(e => e.QuantityInMenu).IsRequired().HasColumnType("decimal(18,2)");

                // Configure many-to-many relationship
                entity.HasOne(md => md.Menu)
                      .WithMany(m => m.MenuDishes)
                      .HasForeignKey(md => md.MenuId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(md => md.Dish)
                      .WithMany(d => d.MenuDishes)
                      .HasForeignKey(md => md.DishId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.DeliveryAddress).HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();
                entity.Property(e => e.Role).IsRequired();
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.OrderCode).IsUnique();
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.FoodCost).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.ShippingCost).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).IsRequired().HasColumnType("decimal(18,2)");

                // Configure one-to-many relationship with User
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ignore computed property
                entity.Ignore(o => o.TotalCost);
            });

            // Configure OrderDetail entity
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");

                // Configure one-to-many relationship with Order
                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Configure optional one-to-many relationship with Dish
                entity.HasOne(od => od.Dish)
                      .WithMany(d => d.OrderDetails)
                      .HasForeignKey(od => od.DishId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // Configure optional one-to-many relationship with Menu
                entity.HasOne(od => od.Menu)
                      .WithMany(m => m.OrderDetails)
                      .HasForeignKey(od => od.MenuId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // Add check constraint: either DishId or MenuId must be non-null, but not both
                entity.HasCheckConstraint("CK_OrderDetail_EitherDishOrMenu",
                    "(DishId IS NULL AND MenuId IS NOT NULL) OR (DishId IS NOT NULL AND MenuId IS NULL)");
            });

            // Set up some default values and seed data (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed allergens
            modelBuilder.Entity<Allergen>().HasData(
                new Allergen { Id = 1, Name = "Gluten", Description = "Found in wheat, barley, and rye" },
                new Allergen { Id = 2, Name = "Eggs", Description = "Found in many baked goods" },
                new Allergen { Id = 3, Name = "Milk", Description = "Lactose and dairy products" },
                new Allergen { Id = 4, Name = "Nuts", Description = "Tree nuts like almonds, walnuts, etc." },
                new Allergen { Id = 5, Name = "Peanuts", Description = "Legume used in many dishes" },
                new Allergen { Id = 6, Name = "Fish", Description = "Various fish species" },
                new Allergen { Id = 7, Name = "Shellfish", Description = "Crustaceans like shrimp, crab, lobster" },
                new Allergen { Id = 8, Name = "Soy", Description = "Soybean products" }
            );

            // Seed categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Breakfast", Description = "Morning meals" },
                new Category { Id = 2, Name = "Appetizers", Description = "Starters and small dishes" },
                new Category { Id = 3, Name = "Soups", Description = "Hot and cold soups" },
                new Category { Id = 4, Name = "Main Courses", Description = "Main dishes" },
                new Category { Id = 5, Name = "Desserts", Description = "Sweet dishes" },
                new Category { Id = 6, Name = "Beverages", Description = "Drinks" }
            );

            // You can add more seed data here if needed
        }
    }
}
using BCrypt.Net;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;
namespace ECommerceApp.Data
{
    public class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Electronics",   Description = "Phones, Laptops, Gadgets",         IconClass = "bi-phone",  ImageUrl = "" },
                    new() { Name = "Fashion",        Description = "Men, Women, Kids Clothing",        IconClass = "bi-bag",    ImageUrl = "" },
                    new() { Name = "Jewelry",        Description = "Rings, Necklaces, Bracelets",      IconClass = "bi-gem",    ImageUrl = "" },
                    new() { Name = "Gifts",          Description = "Special gifts for everyone",       IconClass = "bi-gift",   ImageUrl = "" },
                    new() { Name = "Shoes",          Description = "Sneakers, Heels, Sandals",         IconClass = "bi-boot",   ImageUrl = "" },
                    new() { Name = "Home & Kitchen", Description = "Appliances, Furniture, Decor",     IconClass = "bi-house",  ImageUrl = "" },
                    new() { Name = "Books",          Description = "Fiction, Non-Fiction, Educational", IconClass = "bi-book",  ImageUrl = "" },
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                context.Users.Add(new User
                {
                    FullName = "Admin User",
                    Email = "admin@ecommerce.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin"
                });
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var electronicsId = context.Categories.First(c => c.Name == "Electronics").Id;
                var fashionId = context.Categories.First(c => c.Name == "Fashion").Id;
                var jewelryId = context.Categories.First(c => c.Name == "Jewelry").Id;
                var giftsId = context.Categories.First(c => c.Name == "Gifts").Id;
                var shoesId = context.Categories.First(c => c.Name == "Shoes").Id;
                var homeKitchenId = context.Categories.First(c => c.Name == "Home & Kitchen").Id;
                var booksId = context.Categories.First(c => c.Name == "Books").Id;

                context.Products.AddRange(
                    // Electronics
                    new Product
                    {
                        Name = "Wireless Headphones",
                        Description = "Premium noise-cancelling headphones with 30-hour battery life and deep bass.",
                        Price = 4999,
                        Stock = 50,
                        CategoryId = electronicsId,
                        ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop"
                    },
                    new Product
                    {
                        Name = "Smart Watch",
                        Description = "Track fitness, heart rate, sleep, and notifications right from your wrist.",
                        Price = 8999,
                        Stock = 30,
                        CategoryId = electronicsId,
                        ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&auto=format&fit=crop"
                    },
                    new Product
                    {
                        Name = "Bluetooth Speaker",
                        Description = "Compact 360° waterproof speaker with 12-hour playtime, perfect for outdoors.",
                        Price = 2499,
                        Stock = 40,
                        CategoryId = electronicsId,
                        ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=600&auto=format&fit=crop"
                    },

                    // Fashion
                    new Product
                    {
                        Name = "Men's Classic T-Shirt",
                        Description = "100% breathable cotton, available in multiple colors. Regular fit, sizes S–XXL.",
                        Price = 799,
                        Stock = 100,
                        CategoryId = fashionId,
                        ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=600&auto=format&fit=crop"
                    },
                    new Product
                    {
                        Name = "Women's Floral Dress",
                        Description = "Lightweight summer dress with vibrant floral print. Sizes XS–XL.",
                        Price = 1599,
                        Stock = 75,
                        CategoryId = fashionId,
                        ImageUrl = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=600&auto=format&fit=crop"
                    },

                    // Jewelry
                    new Product
                    {
                        Name = "Gold Necklace",
                        Description = "Elegant 18K gold-plated chain necklace, suitable for everyday wear.",
                        Price = 2999,
                        Stock = 20,
                        CategoryId = jewelryId,
                        ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f?w=600&auto=format&fit=crop"
                    },
                    new Product
                    {
                        Name = "Diamond Stud Earrings",
                        Description = "Classic solitaire diamond stud earrings set in sterling silver, 0.5 ct each.",
                        Price = 5499,
                        Stock = 15,
                        CategoryId = jewelryId,
                        ImageUrl = "https://images.unsplash.com/photo-1588444837495-c6cfeb53f32d?w=600&auto=format&fit=crop"
                    },

                    // Shoes
                    new Product
                    {
                        Name = "Men's Running Sneakers",
                        Description = "Lightweight mesh sneakers with cushioned sole for all-day comfort. Sizes 39–46.",
                        Price = 3499,
                        Stock = 60,
                        CategoryId = shoesId,
                        ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&auto=format&fit=crop"
                    },
                    new Product
                    {
                        Name = "Women's Block Heel Sandals",
                        Description = "Stylish strappy sandals with a stable 5 cm block heel. Available in black and nude.",
                        Price = 1999,
                        Stock = 45,
                        CategoryId = shoesId,
                        ImageUrl = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?w=600&auto=format&fit=crop"
                    },

                    // Home & Kitchen
                    new Product
                    {
                        Name = "Stainless Steel Cookware Set",
                        Description = "5-piece tri-ply stainless steel set including frying pan, saucepan, and stockpot.",
                        Price = 6999,
                        Stock = 25,
                        CategoryId = homeKitchenId,
                        ImageUrl = "https://images.unsplash.com/photo-1584990347449-a2d4c2b9b087?w=600&auto=format&fit=crop"
                    },

                    // Gifts
                    new Product
                    {
                        Name = "Luxury Gift Hamper",
                        Description = "Curated hamper with chocolates, scented candles, and premium skincare items.",
                        Price = 3299,
                        Stock = 35,
                        CategoryId = giftsId,
                        ImageUrl = "https://images.unsplash.com/photo-1607344645866-009c320b63e0?w=600&auto=format&fit=crop"
                    },

                    // Books
                    new Product
                    {
                        Name = "The Art of Thinking Clearly",
                        Description = "A bestselling guide to cognitive biases and better decision-making by Rolf Dobelli.",
                        Price = 599,
                        Stock = 80,
                        CategoryId = booksId,
                        ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&auto=format&fit=crop"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
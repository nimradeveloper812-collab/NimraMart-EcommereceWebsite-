using ECommerceApp.Data;
using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        public CartService(ApplicationDbContext context) => _context = context;

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<CartViewModel> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return new CartViewModel
            {
                Items = cart.Items.Select(i => new CartItemViewModel
                {
                    ProductId   = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    ImageUrl    = i.Product?.ImageUrl,
                    UnitPrice   = i.Product?.Price ?? 0,
                    Quantity    = i.Quantity
                }).ToList()
            };
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                cart.Items.Add(new CartItem { CartId = cart.Id, ProductId = productId, Quantity = quantity });
            else
                item.Quantity += quantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) { _context.CartItems.Remove(item); await _context.SaveChangesAsync(); }
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) { item.Quantity = quantity; await _context.SaveChangesAsync(); }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
        }
    }
}

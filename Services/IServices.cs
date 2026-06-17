using ECommerceApp.Models;
using ECommerceApp.Models.ViewModels;

namespace ECommerceApp.Services
{
    // ─── Interfaces ────────────────────────────────────────────────

    public interface IProductService
    {
        Task<IQueryable<Product>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = null);
        Task<Product?> GetByIdAsync(int id);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);

        Task AddProductImageAsync(ProductImage image);
    }

    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(int userId);
        Task AddToCartAsync(int userId, int productId, int quantity);
        Task RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
        Task UpdateQuantityAsync(int userId, int productId, int quantity);
    }

    public interface IOrderService
    {
        Task<List<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> PlaceOrderAsync(int userId, string shippingAddress,
                                    string paymentMethod, string? last4Digits = null);
        Task UpdateStatusAsync(int orderId, OrderStatus status);
        Task<List<Order>> GetAllOrdersAsync();
    }
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<bool> RegisterGoogleUserAsync(RegisterViewModel model);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
    }

    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
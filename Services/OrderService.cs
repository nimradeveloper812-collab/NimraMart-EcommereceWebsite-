using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<List<Order>> GetUserOrdersAsync(int userId) =>
            await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

        public async Task<Order?> GetOrderByIdAsync(int id) =>
            await _context.Orders
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<Order> PlaceOrderAsync(int userId, string shippingAddress,
                                           string paymentMethod, string? last4Digits = null)
        {
            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new Exception("Cart not found");

            if (!cart.Items.Any()) throw new Exception("Cart is empty");

            var method = paymentMethod == "Card"
                ? Models.PaymentMethod.Card
                : Models.PaymentMethod.CashOnDelivery;

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = shippingAddress,
                PaymentMethod = method,
                PaymentStatus = method == Models.PaymentMethod.Card
                    ? Models.PaymentStatus.Paid
                    : Models.PaymentStatus.Unpaid,
                Last4Digits = last4Digits,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product!.Price
                }).ToList()
            };
            order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);
            return order;
        }
        public async Task UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null) { order.Status = status; await _context.SaveChangesAsync(); }
        }

        public async Task<List<Order>> GetAllOrdersAsync() =>
            await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
    }
}

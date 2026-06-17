using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService) => _orderService = orderService;

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            var orders = await _orderService.GetUserOrdersAsync(UserId.Value);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            if (order.UserId != UserId && HttpContext.Session.GetString("Role") != "Admin")
                return Forbid();
            return View(order);
        }
    }
}

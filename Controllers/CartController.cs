using ECommerceApp.Models.ViewModels;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        private int? UserId => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            var cart = await _cartService.GetCartAsync(UserId.Value);
            ViewBag.CartCount = cart.ItemCount;
            return View(cart);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            await _cartService.AddToCartAsync(UserId.Value, productId, quantity);
            TempData["Success"] = "Item added to cart!";
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            await _cartService.RemoveFromCartAsync(UserId.Value, productId);
            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            if (quantity < 1) quantity = 1;
            await _cartService.UpdateQuantityAsync(UserId.Value, productId, quantity);
            TempData["Success"] = "Quantity updated.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            if (UserId == null) return RedirectToAction("Login", "Account");
            var cart = await _cartService.GetCartAsync(UserId.Value);
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }
            return View(new CheckoutViewModel { Cart = cart });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (UserId == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.ShippingAddress))
            {
                TempData["Error"] = "Please enter a shipping address.";
                return RedirectToAction("Checkout");
            }

            if (model.PaymentMethod == "Card")
            {
                if (string.IsNullOrWhiteSpace(model.CardHolderName) ||
                    string.IsNullOrWhiteSpace(model.CardNumber) ||
                    string.IsNullOrWhiteSpace(model.ExpiryDate) ||
                    string.IsNullOrWhiteSpace(model.CVV))
                {
                    TempData["Error"] = "Please fill in all card details.";
                    return RedirectToAction("Checkout");
                }
            }

            try
            {
                string? last4 = null;
                if (model.PaymentMethod == "Card" && model.CardNumber?.Replace(" ", "").Length >= 4)
                    last4 = model.CardNumber.Replace(" ", "")[^4..];

                var order = await _orderService.PlaceOrderAsync(
                    UserId.Value, model.ShippingAddress, model.PaymentMethod ?? "COD", last4);

                TempData["Success"] = model.PaymentMethod == "Card"
                    ? $"✅ Order #{order.Id} placed! Card payment of Rs. {order.TotalAmount:N0} confirmed."
                    : $"✅ Order #{order.Id} placed! Pay Rs. {order.TotalAmount:N0} on delivery.";

                return RedirectToAction("Details", "Order", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message == "Cart is empty"
                    ? "Your cart is empty."
                    : "Something went wrong. Please try again.";
                return RedirectToAction("Checkout");
            }
        }
    }
}
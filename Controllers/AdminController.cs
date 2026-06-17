using ECommerceApp.Models;
using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _env;

        public AdminController(IProductService productService, IOrderService orderService,
            ICategoryService categoryService, IWebHostEnvironment env)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
            _env = env;
        }

        private bool IsAdmin => HttpContext.Session.GetString("Role") == "Admin";

        private async Task<string?> SaveImageAsync(IFormFile? file, string folder = "products")
        {
            if (file == null || file.Length == 0) return null;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;
            var dir = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/{folder}/{fileName}";
        }

        // ── Dashboard ──────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        // ── Products ───────────────────────────────────────────────
        public async Task<IActionResult> Products()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            return View(await _productService.GetAllAsync());
        }

        public async Task<IActionResult> CreateProduct()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(new Product());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? MainImage, List<IFormFile>? ExtraImages)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var mainUrl = await SaveImageAsync(MainImage);
            if (mainUrl != null) product.ImageUrl = mainUrl;
            await _productService.CreateAsync(product);
            if (ExtraImages != null)
                foreach (var file in ExtraImages)
                {
                    var url = await SaveImageAsync(file);
                    if (url != null)
                        await _productService.AddProductImageAsync(new ProductImage { ProductId = product.Id, ImageUrl = url });
                }
            TempData["Success"] = $"Product '{product.Name}' created successfully.";
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product, IFormFile? MainImage, List<IFormFile>? ExtraImages)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var mainUrl = await SaveImageAsync(MainImage);
            if (mainUrl != null) product.ImageUrl = mainUrl;
            await _productService.UpdateAsync(product);
            if (ExtraImages != null)
                foreach (var file in ExtraImages)
                {
                    var url = await SaveImageAsync(file);
                    if (url != null)
                        await _productService.AddProductImageAsync(new ProductImage { ProductId = product.Id, ImageUrl = url });
                }
            TempData["Success"] = $"Product '{product.Name}' updated successfully.";
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Product deleted.";
            return RedirectToAction("Products");
        }

        // ── Categories ─────────────────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            return View(await _categoryService.GetAllAsync());
        }

        public IActionResult CreateCategory()
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            return View(new Category());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category, IFormFile? CategoryImage)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var url = await SaveImageAsync(CategoryImage, "categories");
            if (url != null) category.ImageUrl = url;
            await _categoryService.CreateAsync(category);
            TempData["Success"] = $"Category '{category.Name}' created.";
            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var cat = await _categoryService.GetByIdAsync(id);
            return cat == null ? NotFound() : View(cat);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category, IFormFile? CategoryImage)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            var url = await SaveImageAsync(CategoryImage, "categories");
            if (url != null) category.ImageUrl = url;
            await _categoryService.UpdateAsync(category);
            TempData["Success"] = $"Category '{category.Name}' updated.";
            return RedirectToAction("Categories");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            await _categoryService.DeleteAsync(id);
            TempData["Success"] = "Category deleted.";
            return RedirectToAction("Categories");
        }

        // ── Orders ─────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            if (!IsAdmin) return RedirectToAction("Login", "Account");
            await _orderService.UpdateStatusAsync(orderId, status);
            TempData["Success"] = $"Order #{orderId} status updated to {status}.";
            return RedirectToAction("Index");
        }
    }
}
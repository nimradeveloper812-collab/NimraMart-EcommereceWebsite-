using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;
using X.PagedList.EF;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, string? sortBy, int page = 1)
        {
            int pageSize = 12;

            ViewBag.CategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.SortBy = sortBy;
            ViewBag.Categories = await _categoryService.GetAllAsync();

            var query = await _productService.GetAllAsync(categoryId, search, sortBy);
            var pagedProducts = await query.ToPagedListAsync(page, pageSize);

            return View(pagedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        public async Task<IActionResult> ByCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();
            ViewBag.Category = category;
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.CategoryId = id;
            var query = await _productService.GetAllAsync(id);
            var pagedProducts = await query.ToPagedListAsync(1, 12);
            return View("Index", pagedProducts);
        }

        // ── Search Suggestions API (returns JSON) ──
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string? q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new { products = Array.Empty<object>(), categories = Array.Empty<object>() });

            var query = await _productService.GetAllAsync(search: q);
            var list = await query.Take(5).ToListAsync();
            var products = list.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price.ToString("N0"),
                category = p.Category != null ? p.Category.Name : "",
                image = p.ImageUrl ?? ""
            });

            var allCategories = await _categoryService.GetAllAsync();
            var categories = allCategories
                .Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .Take(3).Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    icon = c.IconClass ?? "bi-tag",
                    count = c.Products.Count
                });

            return Json(new { products, categories });
        }
    }
}

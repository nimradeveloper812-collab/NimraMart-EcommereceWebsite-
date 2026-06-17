using ECommerceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            var featured = await _productService.GetAllAsync();
            return View(featured.Take(8).ToList());
        }

        public IActionResult Privacy() => View();

        public IActionResult Error()
        {
            var model = new ECommerceApp.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id
                            ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }
    }
}
using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context) => _context = context;

        public Task<IQueryable<Product>> GetAllAsync(int? categoryId = null, string? search = null, string? sortBy = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)) ||
                    (p.Category != null && p.Category.Name.Contains(search)));

            query = sortBy switch
            {
                "price_asc"    => query.OrderBy(p => p.Price),
                "price_desc"   => query.OrderByDescending(p => p.Price),
                "latest"       => query.OrderByDescending(p => p.CreatedAt),
                "best_selling" => query.OrderByDescending(p => _context.OrderItems.Count(oi => oi.ProductId == p.Id)),
                _              => query.OrderBy(p => p.Name)
            };

            return Task.FromResult(query);
        }

        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) { product.IsActive = false; await _context.SaveChangesAsync(); }
        }

        public async Task AddProductImageAsync(ProductImage image)
        {
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();
        }
    }
}

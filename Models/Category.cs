namespace ECommerceApp.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconClass { get; set; }   // Bootstrap icon e.g. "bi-phone"
        public string? ImageUrl { get; set; }    // Category image
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
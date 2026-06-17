namespace ECommerceApp.Models
{
    public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }
    public enum PaymentMethod { CashOnDelivery, Card }
    public enum PaymentStatus { Unpaid, Paid }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── NEW PAYMENT FIELDS ──────────────────────────────
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
        public string? Last4Digits { get; set; }   // stored only for display, never full card
    }
}
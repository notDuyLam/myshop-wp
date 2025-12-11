namespace myshop_data.Models;

public enum OrderStatus
{
    Created,
    Paid,
    Cancelled
}

public class Order
{
    public int OrderId { get; set; }
    public DateTime CreatedTime { get; set; }
    public int FinalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    
    // Navigation property
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}


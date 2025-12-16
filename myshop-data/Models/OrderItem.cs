namespace myshop_data.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitSalePrice { get; set; }
    public int TotalPrice { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


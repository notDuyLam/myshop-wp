namespace myshop_data.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ImportPrice { get; set; }
    public int Count { get; set; }
    public string? Description { get; set; }
    
    // Foreign key
    public int CategoryId { get; set; }
    
    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}


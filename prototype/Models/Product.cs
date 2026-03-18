using System.ComponentModel.DataAnnotations;

namespace prototype.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Required]
    public string Category { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public string? Description { get; set; }
}

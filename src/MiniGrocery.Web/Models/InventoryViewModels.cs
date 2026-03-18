namespace MiniGrocery.Web.Models;

public class ProductInventoryViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsLowStock { get; set; }
}

public class InventoryTransactionViewModel
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public int? ReferenceId { get; set; }
    public string Note { get; set; } = string.Empty;
    public string ByUserId { get; set; } = string.Empty;
}

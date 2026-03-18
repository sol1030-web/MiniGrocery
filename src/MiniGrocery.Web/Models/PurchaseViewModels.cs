using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Models;

public class CreatePurchaseViewModel
{
    public List<Supplier> Suppliers { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<PurchaseItemDto> Items { get; set; } = new();
}

public class PurchaseItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class PurchaseHistoryViewModel
{
    public List<PurchaseTransaction> Transactions { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
}

public class CreatePurchaseRequest
{
    public int SupplierId { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = new();
}

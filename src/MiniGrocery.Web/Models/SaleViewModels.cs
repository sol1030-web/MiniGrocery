using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Models;

public class CreateSaleViewModel
{
    public List<Product> AvailableProducts { get; set; } = new();
    public List<SaleItemDto> Items { get; set; } = new();
}

public class SaleItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class TransactionHistoryViewModel
{
    public List<SaleTransaction> Transactions { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
}

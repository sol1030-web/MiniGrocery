namespace MiniGrocery.Web.Models;

public class InventoryHistoryPageViewModel
{
    public List<InventoryTransactionViewModel> Items { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
}

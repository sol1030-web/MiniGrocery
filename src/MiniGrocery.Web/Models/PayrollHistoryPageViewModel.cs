namespace MiniGrocery.Web.Models;

public class PayrollHistoryPageViewModel
{
    public List<PayrollHistoryViewModel> Items { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
}

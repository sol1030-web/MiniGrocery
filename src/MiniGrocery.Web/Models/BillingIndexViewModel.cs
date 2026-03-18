namespace MiniGrocery.Web.Models;

public class BillingIndexViewModel
{
    public string Status { get; set; } = "All";
    public List<InvoiceViewModel> Items { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();
}

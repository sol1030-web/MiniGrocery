namespace MiniGrocery.Web.Models;

public class InvoiceViewModel
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => TotalAmount - PaidAmount;
    public string Status { get; set; } = string.Empty;
}

public class InvoiceDetailViewModel : InvoiceViewModel
{
    public List<SaleItemViewModel> Items { get; set; } = new();
    public List<PaymentViewModel> Payments { get; set; } = new();
    public decimal CashTendered { get; set; }
    public decimal Change => CashTendered > TotalAmount ? CashTendered - TotalAmount : 0;
    public decimal? ExchangeRateUsdPhp { get; set; }
}

public class SaleItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class PaymentViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class MakePaymentViewModel
{
    public int InvoiceId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? ReferenceNumber { get; set; }
}

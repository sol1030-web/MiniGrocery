using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class Invoice
{
    public int Id { get; set; }

    public int SaleTransactionId { get; set; }
    public SaleTransaction? SaleTransaction { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; 

    public List<PaymentTransaction> PaymentTransactions { get; set; } = new();
}

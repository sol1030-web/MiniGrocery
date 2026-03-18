using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class PurchaseTransaction
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public List<PurchaseItem> PurchaseItems { get; set; } = new();
}

using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class SaleTransaction
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public List<SaleItem> SaleItems { get; set; } = new();
}

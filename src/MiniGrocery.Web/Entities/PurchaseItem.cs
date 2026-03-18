using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class PurchaseItem
{
    public int Id { get; set; }

    public int PurchaseTransactionId { get; set; }
    public PurchaseTransaction? PurchaseTransaction { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
}

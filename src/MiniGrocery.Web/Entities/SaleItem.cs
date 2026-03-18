using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class SaleItem
{
    public int Id { get; set; }
    
    public int SaleTransactionId { get; set; }
    public SaleTransaction? SaleTransaction { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}

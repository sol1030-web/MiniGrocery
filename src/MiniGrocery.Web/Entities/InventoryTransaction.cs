using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty; 

    public int QuantityChange { get; set; } 

    public int? ReferenceId { get; set; } 

    [StringLength(255)]
    public string Note { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
}

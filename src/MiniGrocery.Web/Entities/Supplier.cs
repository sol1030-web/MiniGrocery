using System.ComponentModel.DataAnnotations;

namespace MiniGrocery.Web.Entities;

public class Supplier
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniGrocery.Web.Entities;

public class EmployeeDetails
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
}

using System.ComponentModel.DataAnnotations;
using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Models;

public class EmployeePayrollViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public DateTime JoinDate { get; set; }
    public bool HasDetails { get; set; }
}

public class UpdateEmployeeDetailsModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    public string Department { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
}

public class CreatePayrollViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    public DateTime PayPeriodStart { get; set; }
    
    [Required]
    public DateTime PayPeriodEnd { get; set; }
    
    public decimal BasicSalary { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Bonuses { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Deductions { get; set; }
    
    public decimal NetPay => BasicSalary + Bonuses - Deductions;
}

public class PayrollHistoryViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = string.Empty;
}

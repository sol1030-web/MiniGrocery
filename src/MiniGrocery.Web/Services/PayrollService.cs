using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Entities;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class PayrollService
{
    private readonly ApplicationDbContext _context;

    public PayrollService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeePayrollViewModel>> GetAllEmployeesAsync()
    {
        var users = await _context.Users.ToListAsync();
        var details = await _context.EmployeeDetails.ToDictionaryAsync(d => d.UserId);

        return users.Select(u => {
            var hasDetails = details.TryGetValue(u.Id, out var detail);
            return new EmployeePayrollViewModel
            {
                UserId = u.Id,
                UserName = u.UserName ?? "Unknown",
                Email = u.Email ?? "",
                Position = hasDetails ? detail.Position : "Not Set",
                Department = hasDetails ? detail.Department : "Not Set",
                BaseSalary = hasDetails ? detail.BaseSalary : 0,
                JoinDate = hasDetails ? detail.JoinDate : DateTime.MinValue,
                HasDetails = hasDetails
            };
        }).ToList();
    }

    public async Task<EmployeePayrollViewModel?> GetEmployeeAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        var detail = await _context.EmployeeDetails.FirstOrDefaultAsync(d => d.UserId == userId);

        return new EmployeePayrollViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? "Unknown",
            Email = user.Email ?? "",
            Position = detail?.Position ?? "",
            Department = detail?.Department ?? "",
            BaseSalary = detail?.BaseSalary ?? 0,
            JoinDate = detail?.JoinDate ?? DateTime.UtcNow,
            HasDetails = detail != null
        };
    }

    public async Task UpdateEmployeeDetailsAsync(UpdateEmployeeDetailsModel model)
    {
        var detail = await _context.EmployeeDetails.FirstOrDefaultAsync(d => d.UserId == model.UserId);
        
        if (detail == null)
        {
            detail = new EmployeeDetails
            {
                UserId = model.UserId,
                Position = model.Position,
                Department = model.Department,
                BaseSalary = model.BaseSalary,
                JoinDate = DateTime.UtcNow
            };
            _context.EmployeeDetails.Add(detail);
        }
        else
        {
            detail.Position = model.Position;
            detail.Department = model.Department;
            detail.BaseSalary = model.BaseSalary;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreatePayrollTransactionAsync(CreatePayrollViewModel model)
    {
        var transaction = new PayrollTransaction
        {
            UserId = model.UserId,
            PayPeriodStart = model.PayPeriodStart,
            PayPeriodEnd = model.PayPeriodEnd,
            PaymentDate = DateTime.UtcNow,
            BasicSalary = model.BasicSalary,
            Bonuses = model.Bonuses,
            Deductions = model.Deductions,
            NetPay = model.BasicSalary + model.Bonuses - model.Deductions,
            Status = "Processed"
        };

        _context.PayrollTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PayrollHistoryViewModel>> GetPayrollHistoryAsync()
    {
        return await _context.PayrollTransactions
            .Include(pt => pt.User)
            .OrderByDescending(pt => pt.PaymentDate)
            .Select(pt => new PayrollHistoryViewModel
            {
                Id = pt.Id,
                EmployeeName = pt.User.UserName ?? "Unknown",
                PaymentDate = pt.PaymentDate,
                PayPeriodStart = pt.PayPeriodStart,
                PayPeriodEnd = pt.PayPeriodEnd,
                NetPay = pt.NetPay,
                Status = pt.Status
            })
            .ToListAsync();
    }

    public async Task<(List<PayrollHistoryViewModel> items, int totalCount)> GetPayrollHistoryPageAsync(int page, int pageSize)
    {
        var query = _context.PayrollTransactions
            .Include(pt => pt.User)
            .OrderByDescending(pt => pt.PaymentDate)
            .Select(pt => new PayrollHistoryViewModel
            {
                Id = pt.Id,
                EmployeeName = pt.User.UserName ?? "Unknown",
                PaymentDate = pt.PaymentDate,
                PayPeriodStart = pt.PayPeriodStart,
                PayPeriodEnd = pt.PayPeriodEnd,
                NetPay = pt.NetPay,
                Status = pt.Status
            });
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}

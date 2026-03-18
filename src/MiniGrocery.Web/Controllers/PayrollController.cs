using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Models;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,System Administrator")]
public class PayrollController : Controller
{
    private readonly PayrollService _payrollService;

    public PayrollController(PayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _payrollService.GetAllEmployeesAsync();
        return View(employees);
    }

    [HttpGet]
    public async Task<IActionResult> New()
    {
        var employees = await _payrollService.GetAllEmployeesAsync();
        // Only show employees with completed details to proceed to payroll
        var eligible = employees.Where(e => e.HasDetails).ToList();
        return View(eligible);
    }

    [HttpGet]
    public async Task<IActionResult> Manage(string id)
    {
        var employee = await _payrollService.GetEmployeeAsync(id);
        if (employee == null) return NotFound();

        var model = new UpdateEmployeeDetailsModel
        {
            UserId = employee.UserId,
            Position = employee.Position == "Not Set" ? "" : employee.Position,
            Department = employee.Department == "Not Set" ? "" : employee.Department,
            BaseSalary = employee.BaseSalary
        };

        ViewBag.UserName = employee.UserName;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Manage(UpdateEmployeeDetailsModel model)
    {
        if (ModelState.IsValid)
        {
            await _payrollService.UpdateEmployeeDetailsAsync(model);
            TempData["ToastSuccess"] = "Payroll details updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string id)
    {
        var employee = await _payrollService.GetEmployeeAsync(id);
        if (employee == null) return NotFound();

        var model = new CreatePayrollViewModel
        {
            UserId = employee.UserId,
            UserName = employee.UserName,
            BasicSalary = employee.BaseSalary,
            PayPeriodStart = DateTime.Today.AddMonths(-1),
            PayPeriodEnd = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePayrollViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _payrollService.CreatePayrollTransactionAsync(model);
            if (User.IsInRole("System Administrator"))
            {
                TempData["ToastSuccess"] = "Payroll created successfully.";
                return RedirectToAction(nameof(History));
            }
            else
            {
                TempData["ToastSuccess"] = "Payroll processed successfully.";
                return RedirectToAction(nameof(Index));
            }
        }
        return View(model);
    }

    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> History(int page = 1, int pageSize = 10)
    {
        var (items, total) = await _payrollService.GetPayrollHistoryPageAsync(page, pageSize);
        return View(new PayrollHistoryPageViewModel
        {
            Items = items,
            Pagination = new PaginationModel { Page = page, PageSize = pageSize, TotalCount = total }
        });
    }
}

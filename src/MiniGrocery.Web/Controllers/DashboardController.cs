using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,Sales Staff,System Administrator")]
public class DashboardController : Controller
{
    private readonly AnalyticsService _analyticsService;
    private readonly IHolidayService _holidayService;

    public DashboardController(AnalyticsService analyticsService, IHolidayService holidayService)
    {
        _analyticsService = analyticsService;
        _holidayService = holidayService;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _analyticsService.GetDashboardDataAsync();
        return View(data);
    }

    [HttpGet]
    [Route("api/holidays")]
    public async Task<IActionResult> GetHolidays(int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var list1 = await _holidayService.GetHolidaysAsync(y, HttpContext.RequestAborted);
        var list2 = await _holidayService.GetHolidaysAsync(y + 1, HttpContext.RequestAborted);
        var data = list1.Concat(list2).OrderBy(h => h.Date).ToList();
        return Json(data);
    }
}

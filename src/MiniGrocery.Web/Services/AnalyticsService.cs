using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class AnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IExchangeRatesService _fx;
    private readonly IHolidayService _holidays;

    public AnalyticsService(ApplicationDbContext context, IExchangeRatesService fx, IHolidayService holidays)
    {
        _context = context;
        _fx = fx;
        _holidays = holidays;
    }

    public async Task<AnalyticsDashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateTime.UtcNow.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var todaySales = await _context.SaleTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= today)
            .SumAsync(t => t.TotalAmount);

        var monthSales = await _context.SaleTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= startOfMonth)
            .SumAsync(t => t.TotalAmount);

        var monthPurchases = await _context.PurchaseTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= startOfMonth)
            .SumAsync(t => t.TotalCost);

        var monthPayrollQuery = _context.PayrollTransactions
            .AsNoTracking()
            .Where(t => t.PaymentDate >= startOfMonth && t.Status == "Processed");
        var excludeStatuses = (Environment.GetEnvironmentVariable("DEMO_EXCLUDE_PAYROLL_STATUS") ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (excludeStatuses.Count > 0)
        {
            monthPayrollQuery = monthPayrollQuery.Where(t => !excludeStatuses.Contains(t.Status));
        }
        if (DateTime.TryParse(Environment.GetEnvironmentVariable("DEMO_EXCLUDE_PAYROLL_BEFORE"), out var cutoff) && cutoff > DateTime.MinValue)
        {
            monthPayrollQuery = monthPayrollQuery.Where(t => t.PaymentDate >= cutoff);
        }
        var monthPayroll = await monthPayrollQuery.SumAsync(t => t.NetPay);

        var totalRevenueCollected = await _context.PaymentTransactions
            .AsNoTracking()
            .Where(t => t.PaymentDate >= startOfMonth)
            .SumAsync(t => t.Amount);

        var outstandingBalanceRows = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status != "Paid")
            .Select(i => new { i.TotalAmount, i.PaidAmount })
            .ToListAsync();
        var outstandingBalance = outstandingBalanceRows.Sum(r => r.TotalAmount - r.PaidAmount);

        var paymentMethods = await _context.PaymentTransactions
            .AsNoTracking()
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodDto
            {
                Method = g.Key,
                TotalAmount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .ToListAsync();

        var topProducts = await _context.SaleItems
            .AsNoTracking()
            .GroupBy(i => i.ProductId)
            .Select(g => new TopProductDto
            {
                ProductName = g.First().Product.Name,
                TotalQuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice)
            })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(5)
            .ToListAsync();

        var supplierPerformance = await _context.PurchaseTransactions
            .AsNoTracking()
            .GroupBy(t => t.SupplierId)
            .Select(g => new SupplierPerformanceDto
            {
                SupplierName = g.First().Supplier.Name,
                TotalCost = g.Sum(t => t.TotalCost),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalCost)
            .ToListAsync();

        var userPerformance = await _context.SaleTransactions
            .AsNoTracking()
            .GroupBy(t => t.UserId)
            .Select(g => new UserPerformanceDto
            {
                UserName = g.First().User.UserName ?? "Unknown",
                TotalSalesAmount = g.Sum(t => t.TotalAmount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(x => x.TotalSalesAmount)
            .ToListAsync();

       
        var endOfMonth = startOfMonth.AddMonths(1);
        var monthlySalesRows = await _context.SaleTransactions
            .AsNoTracking()
            .Where(t => t.TransactionDate >= startOfMonth && t.TransactionDate < endOfMonth)
            .Select(t => new { Date = t.TransactionDate.Date, Amount = t.TotalAmount })
            .ToListAsync();

        var rawMap = monthlySalesRows
            .GroupBy(r => r.Date)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
        var dayCount = (endOfMonth - startOfMonth).Days;
        var dailyTrend = Enumerable.Range(0, dayCount)
            .Select(offset => startOfMonth.AddDays(offset).Date)
            .Select(d => new DailySalesDto
            {
                Date = d,
                TotalAmount = rawMap.TryGetValue(d, out var amt) ? amt : 0m
            })
            .ToList();

        var inventorySummary = new InventorySummaryDto
        {
            TotalProducts = await _context.Products.CountAsync(),
            LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= 10),
            OutOfStockCount = await _context.Products.CountAsync(p => p.StockQuantity == 0),
            TotalInventoryValue = await _context.Products.SumAsync(p => p.StockQuantity * p.Price)
        };

        var fx = await _fx.GetLatestAsync(CancellationToken.None);
        var holidaysThisYear = await _holidays.GetHolidaysAsync(today.Year, CancellationToken.None);
        var holidaysNextYear = await _holidays.GetHolidaysAsync(today.Year + 1, CancellationToken.None);
        var holidayList = holidaysThisYear.Concat(holidaysNextYear).OrderBy(h => h.Date).ToList();
        var upcoming = holidayList.Where(h => h.Date >= today).Take(3).ToList();

        return new AnalyticsDashboardViewModel
        {
            TodaySales = todaySales,
            MonthSales = monthSales,
            MonthPurchases = monthPurchases,
            MonthPayroll = monthPayroll,
            TotalRevenueCollected = totalRevenueCollected,
            OutstandingBalance = outstandingBalance,
            TopProducts = topProducts,
            UserPerformance = userPerformance,
            DailyTrend = dailyTrend,
            SupplierPerformance = supplierPerformance,
            PaymentMethods = paymentMethods,
            InventorySummary = inventorySummary,
            FxUsdRate = fx.usd,
            FxEurRate = fx.eur,
            UpcomingHolidays = upcoming
        };
    }
}

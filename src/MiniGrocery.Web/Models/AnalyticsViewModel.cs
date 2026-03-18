using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Models;

public class AnalyticsDashboardViewModel
{
    public decimal TodaySales { get; set; }
    public decimal MonthSales { get; set; }
    public decimal MonthPurchases { get; set; } 
    public decimal MonthPayroll { get; set; } 
    public decimal TotalRevenueCollected { get; set; } 
    public decimal OutstandingBalance { get; set; } 
    public List<TopProductDto> TopProducts { get; set; } = new();
    public List<UserPerformanceDto> UserPerformance { get; set; } = new();
    public List<DailySalesDto> DailyTrend { get; set; } = new();
    public List<SupplierPerformanceDto> SupplierPerformance { get; set; } = new(); 
    public List<PaymentMethodDto> PaymentMethods { get; set; } = new(); 
    public InventorySummaryDto InventorySummary { get; set; } = new(); 
    public decimal FxUsdRate { get; set; }
    public decimal FxEurRate { get; set; }
    public List<HolidayDto> UpcomingHolidays { get; set; } = new();
}

public class InventorySummaryDto
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int OutOfStockCount { get; set; }
}

public class PaymentMethodDto
{
    public string Method { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}

public class SupplierPerformanceDto
{
    public string SupplierName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int TransactionCount { get; set; }
}

public class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class UserPerformanceDto
{
    public string UserName { get; set; } = string.Empty;
    public decimal TotalSalesAmount { get; set; }
    public int TransactionCount { get; set; }
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
}

public class HolidayDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
}

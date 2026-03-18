using Microsoft.AspNetCore.Mvc;
using prototype.Models;

namespace prototype.Controllers;

public class SalesController : Controller
{
    // Mock data for display
    private static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Apple", Price = 0.50m, StockQuantity = 120, Category = "Fruits" },
        new Product { Id = 2, Name = "Banana", Price = 0.30m, StockQuantity = 149, Category = "Fruits" },
        new Product { Id = 3, Name = "Milk", Price = 2.50m, StockQuantity = 47, Category = "Dairy" },
        new Product { Id = 4, Name = "Bread", Price = 1.50m, StockQuantity = 60, Category = "Bakery" },
        new Product { Id = 5, Name = "Eggs (Dozen)", Price = 3.00m, StockQuantity = 39, Category = "Dairy" },
        new Product { Id = 6, Name = "Soda Can", Price = 1.00m, StockQuantity = 197, Category = "Beverages" },
        new Product { Id = 7, Name = "Chips", Price = 1.25m, StockQuantity = 100, Category = "Snacks" },
        new Product { Id = 8, Name = "Water Bottle", Price = 0.75m, StockQuantity = 300, Category = "Beverages" }
    };

    public IActionResult Create()
    {
        ViewData["Title"] = "New Sale (POS)";
        return View(_products);
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Sales History";
        return View();
    }
}

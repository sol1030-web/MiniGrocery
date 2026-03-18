using Microsoft.AspNetCore.Mvc;
using prototype.Models;

namespace prototype.Controllers;

public class ProductController : Controller
{
    private static List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "Apple", Price = 15.00m, Category = "Fruits", StockQuantity = 100, Description = "Fresh red apples" },
        new Product { Id = 2, Name = "Bread", Price = 45.00m, Category = "Bakery", StockQuantity = 20, Description = "Whole wheat bread" },
        new Product { Id = 3, Name = "Milk", Price = 95.00m, Category = "Dairy", StockQuantity = 15, Description = "1L Fresh milk" }
    };

    public IActionResult Index()
    {
        return View(_products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            product.Id = _products.Max(p => p.Id) + 1;
            _products.Add(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    public IActionResult Edit(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost]
    public IActionResult Edit(Product product)
    {
        if (ModelState.IsValid)
        {
            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Category = product.Category;
                existing.StockQuantity = product.StockQuantity;
                existing.Description = product.Description;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return RedirectToAction(nameof(Index));
    }
}

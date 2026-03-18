using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,Inventory Clerk,System Administrator")]
public class InventoryController : Controller
{
    private readonly InventoryService _inventoryService;
    private readonly IWebHostEnvironment _env;

    public InventoryController(InventoryService inventoryService, IWebHostEnvironment env)
    {
        _inventoryService = inventoryService;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var inventory = await _inventoryService.GetInventorySummaryAsync();
        return View(inventory);
    }

    [Authorize(Roles = "Manager,Inventory Clerk,System Administrator")]
    public async Task<IActionResult> Archived()
    {
        var archived = await _inventoryService.GetArchivedProductsAsync();
        return View(archived);
    }

    [Authorize(Roles = "System Administrator")]
    public async Task<IActionResult> History(int page = 1, int pageSize = 10)
    {
        var (items, total) = await _inventoryService.GetHistoryPageAsync(page, pageSize);
        var model = new MiniGrocery.Web.Models.InventoryHistoryPageViewModel
        {
            Items = items,
            Pagination = new MiniGrocery.Web.Models.PaginationModel { Page = page, PageSize = pageSize, TotalCount = total }
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Inventory Clerk,System Administrator,Manager")]
    public async Task<IActionResult> AdjustQuantity(int id, int newQuantity, decimal unitPrice)
    {
        var userId = User?.Identity?.Name ?? "system";
        await _inventoryService.SetStockQuantityAsync(id, newQuantity, userId);
        await _inventoryService.UpdateProductPriceAsync(id, unitPrice, userId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Inventory Clerk,System Administrator,Manager")]
    public async Task<IActionResult> Archive(int id)
    {
        var userId = User?.Identity?.Name ?? "system";
        await _inventoryService.ArchiveProductAsync(id, userId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Inventory Clerk,System Administrator,Manager")]
    public async Task<IActionResult> Restore(int id)
    {
        var userId = User?.Identity?.Name ?? "system";
        await _inventoryService.RestoreProductAsync(id, userId);
        TempData["ToastSuccess"] = "Product restored successfully.";
        return RedirectToAction(nameof(Archived));
    }

    [HttpPost]
    [Authorize(Roles = "Inventory Clerk,System Administrator,Manager")]
    public async Task<IActionResult> AddProduct(string name, string? category, decimal price, int stockQuantity, string? description, IFormFile? imageFile)
    {
        try
        {
            var userId = User?.Identity?.Name ?? "system";
            var product = await _inventoryService.CreateProductAsync(name, category, price, stockQuantity, description, userId);

            if (imageFile != null && imageFile.Length > 0)
            {
                var allowed = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                    throw new InvalidOperationException("Unsupported image type.");
                if (imageFile.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("Image file too large.");

                var dir = Path.Combine(_env.WebRootPath, "img", "products");
                Directory.CreateDirectory(dir);
                var filePath = Path.Combine(dir, $"product_{product.Id}{ext}");
                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["InventoryError"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}

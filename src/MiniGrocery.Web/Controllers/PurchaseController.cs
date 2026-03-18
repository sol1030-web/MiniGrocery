using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Models;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,Inventory Clerk,System Administrator")]
public class PurchaseController : Controller
{
    private readonly PurchaseService _purchaseService;
    private readonly SalesService _salesService; 

    public PurchaseController(PurchaseService purchaseService, SalesService salesService)
    {
        _purchaseService = purchaseService;
        _salesService = salesService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var (items, total) = await _purchaseService.GetPurchaseHistoryPageAsync(page, pageSize);
        return View(new PurchaseHistoryViewModel
        {
            Transactions = items,
            Pagination = new PaginationModel { Page = page, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<IActionResult> Create()
    {
        var suppliers = await _purchaseService.GetAllSuppliersAsync();
        var products = await _salesService.GetAllProductsAsync();
        
        var model = new CreatePurchaseViewModel
        {
            Suppliers = suppliers,
            Products = products
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequest request)
    {
        if (request == null || request.Items == null || !request.Items.Any() || request.SupplierId <= 0)
        {
            return BadRequest("Invalid purchase data.");
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var transaction = await _purchaseService.CreatePurchaseAsync(userId, request.SupplierId, request.Items);
            return Ok(new { message = "Purchase transaction created successfully", transactionId = transaction.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var txn = await _purchaseService.GetPurchaseDetailsAsync(id);
        if (txn == null) return NotFound();
        return View(txn);
    }
}

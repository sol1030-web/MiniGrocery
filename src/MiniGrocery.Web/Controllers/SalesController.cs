using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Models;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,Sales Staff,System Administrator")]
public class SalesController : Controller
{
    private readonly SalesService _salesService;
    private readonly BillingService _billingService;

    public SalesController(SalesService salesService, BillingService billingService)
    {
        _salesService = salesService;
        _billingService = billingService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
    {
        var (items, total) = await _salesService.GetTransactionHistoryPageAsync(page, pageSize);
        return View(new TransactionHistoryViewModel
        {
            Transactions = items,
            Pagination = new PaginationModel { Page = page, PageSize = pageSize, TotalCount = total }
        });
    }

    public async Task<IActionResult> Create()
    {
        var products = await _salesService.GetAllProductsAsync();
        var model = new CreateSaleViewModel
        {
            AvailableProducts = products
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        if (request == null || request.Items == null || !request.Items.Any())
        {
            return BadRequest("Invalid sales data.");
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var transaction = await _salesService.CreateTransactionAsync(userId, request.Items);
            var invoice = await _billingService.CreateInvoiceForSaleAsync(transaction.Id, transaction.TotalAmount, markAsPaid: true, paymentMethod: "Cash", cashTendered: request.PaymentAmount);
            return Ok(new { message = "Transaction created successfully", transactionId = transaction.Id, invoiceId = invoice.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateSaleRequest
{
    public List<SaleItemDto> Items { get; set; } = new();
    [System.Text.Json.Serialization.JsonPropertyName("paymentAmount")]
    public decimal PaymentAmount { get; set; }
}

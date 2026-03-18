using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniGrocery.Web.Models;
using MiniGrocery.Web.Services;

namespace MiniGrocery.Web.Controllers;

[Authorize(Roles = "Manager,Sales Staff,System Administrator")]
public class BillingController : Controller
{
    private readonly BillingService _billingService;
    private readonly IExchangeRatesService _fxService;

    public BillingController(BillingService billingService, IExchangeRatesService fxService)
    {
        _billingService = billingService;
        _fxService = fxService;
    }

    public async Task<IActionResult> Index(string status = "All", int page = 1, int pageSize = 10)
    {
        var (items, total) = await _billingService.GetInvoicesPageAsync(status, page, pageSize);
        var model = new BillingIndexViewModel
        {
            Status = status,
            Items = items,
            Pagination = new PaginationModel { Page = page, PageSize = pageSize, TotalCount = total }
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _billingService.GetInvoiceDetailsAsync(id);
        if (invoice == null) return NotFound();
        return RedirectToAction(nameof(Receipt), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int id)
    {
        var invoice = await _billingService.GetInvoiceDetailsAsync(id);
        if (invoice == null) return NotFound();

        var model = new MakePaymentViewModel
        {
            InvoiceId = invoice.Id,
            TotalAmount = invoice.TotalAmount,
            PendingBalance = invoice.Balance,
            PaymentAmount = invoice.Balance // Default to full amount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var invoice = await _billingService.GetInvoiceDetailsAsync(id);
        if (invoice == null) return NotFound();
        try
        {
            var fx = await _fxService.GetLatestAsync(HttpContext.RequestAborted);
            invoice.ExchangeRateUsdPhp = fx.usd > 0 ? fx.usd : null;
        }
        catch
        {
            invoice.ExchangeRateUsdPhp = null;
        }
        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(MakePaymentViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (model.PaymentAmount <= 0)
                {
                    ModelState.AddModelError("", "Payment amount must be greater than zero.");
                    return View(model);
                }
                
                if (model.PaymentAmount > model.PendingBalance)
                {
                    ModelState.AddModelError("", "Payment amount cannot exceed pending balance.");
                    return View(model);
                }

                await _billingService.ProcessPaymentAsync(model.InvoiceId, model.PaymentAmount, model.PaymentMethod, model.ReferenceNumber ?? "");
                return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(model);
    }
}

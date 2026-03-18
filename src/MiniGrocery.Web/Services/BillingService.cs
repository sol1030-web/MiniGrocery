using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Entities;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class BillingService
{
    private readonly ApplicationDbContext _context;

    public BillingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoiceForSaleAsync(int saleId, decimal amount, bool markAsPaid = false, string paymentMethod = "Cash", decimal cashTendered = 0)
    {
        // Check if invoice already exists
        var existing = await _context.Invoices.FirstOrDefaultAsync(i => i.SaleTransactionId == saleId);
        if (existing != null)
        {
            if (markAsPaid && existing.PaidAmount < existing.TotalAmount)
            {
                var remaining = existing.TotalAmount - existing.PaidAmount;

                var paymentForExisting = new PaymentTransaction
                {
                    InvoiceId = existing.Id,
                    Amount = remaining,
                    PaymentMethod = paymentMethod,
                    ReferenceNumber = "POS Auto Payment",
                    PaymentDate = DateTime.UtcNow
                };

                _context.PaymentTransactions.Add(paymentForExisting);

                existing.PaidAmount = existing.TotalAmount;
                existing.Status = "Paid";

                await _context.SaveChangesAsync();
            }

            return existing;
        }

        var invoice = new Invoice
        {
            SaleTransactionId = saleId,
            TotalAmount = amount,
            PaidAmount = markAsPaid ? amount : 0,
            Status = markAsPaid ? "Paid" : "Pending",
            InvoiceDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30) // Default 30 days term
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        if (markAsPaid)
        {
            var payment = new PaymentTransaction
            {
                InvoiceId = invoice.Id,
                Amount = cashTendered > 0 ? cashTendered : amount,
                PaymentMethod = paymentMethod,
                ReferenceNumber = "POS Auto Payment",
                PaymentDate = DateTime.UtcNow
            };

            _context.PaymentTransactions.Add(payment);
            await _context.SaveChangesAsync();
        }

        return invoice;
    }

    public async Task<List<InvoiceViewModel>> GetInvoicesAsync(string status = "All")
    {
        var query = _context.Invoices
            .Include(i => i.SaleTransaction)
            .ThenInclude(s => s.User)
            .AsQueryable();

        if (status != "All")
        {
            query = query.Where(i => i.Status == status);
        }

        return await query
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new InvoiceViewModel
            {
                Id = i.Id,
                SaleId = i.SaleTransactionId,
                CustomerName = i.SaleTransaction.User.FullName ?? i.SaleTransaction.User.UserName ?? "Unknown",
                Date = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status
            })
            .ToListAsync();
    }

    public async Task<(List<InvoiceViewModel> items, int totalCount)> GetInvoicesPageAsync(string status, int page, int pageSize)
    {
        var query = _context.Invoices
            .Include(i => i.SaleTransaction)
            .ThenInclude(s => s.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && status != "All")
        {
            query = query.Where(i => i.Status == status);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InvoiceViewModel
            {
                Id = i.Id,
                SaleId = i.SaleTransactionId,
                CustomerName = i.SaleTransaction.User.FullName ?? i.SaleTransaction.User.UserName ?? "Unknown",
                Date = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status
            })
            .ToListAsync();
        return (items, total);
    }

    public async Task<InvoiceDetailViewModel?> GetInvoiceDetailsAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.PaymentTransactions)
            .Include(i => i.SaleTransaction)
            .ThenInclude(s => s.User)
            .Include(i => i.SaleTransaction)
            .ThenInclude(s => s.SaleItems)
            .ThenInclude(si => si.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return null;

        return new InvoiceDetailViewModel
        {
            Id = invoice.Id,
            SaleId = invoice.SaleTransactionId,
            CustomerName = invoice.SaleTransaction.User.FullName ?? invoice.SaleTransaction.User.UserName ?? "Unknown",
            Date = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            Status = invoice.Status,
            Items = invoice.SaleTransaction.SaleItems.Select(si => new SaleItemViewModel
            {
                ProductName = si.Product.Name,
                Quantity = si.Quantity,
                UnitPrice = si.UnitPrice,
                Total = si.Quantity * si.UnitPrice
            }).ToList(),
            Payments = invoice.PaymentTransactions.Select(pt => new PaymentViewModel
            {
                Id = pt.Id,
                Date = pt.PaymentDate,
                Amount = pt.Amount,
                Method = pt.PaymentMethod,
                Reference = pt.ReferenceNumber
            }).ToList(),
            CashTendered = invoice.PaymentTransactions
                .Where(p => p.PaymentMethod == "Cash" && p.ReferenceNumber == "POS Auto Payment")
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault()?.Amount ?? 0
        };
    }

    public async Task ProcessPaymentAsync(int invoiceId, decimal amount, string method, string reference)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null) throw new Exception("Invoice not found");

        var payment = new PaymentTransaction
        {
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = method,
            ReferenceNumber = reference,
            PaymentDate = DateTime.UtcNow
        };

        _context.PaymentTransactions.Add(payment);

        // Update Invoice Status
        invoice.PaidAmount += amount;
        
        if (invoice.PaidAmount >= invoice.TotalAmount)
        {
            invoice.Status = "Paid";
        }
        else
        {
            invoice.Status = "Partially Paid";
        }

        await _context.SaveChangesAsync();
    }
}

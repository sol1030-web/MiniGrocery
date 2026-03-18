using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Entities;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class SalesService
{
    private readonly ApplicationDbContext _context;
    private readonly InventoryService _inventoryService;
    private readonly BillingService _billingService;

    public SalesService(ApplicationDbContext context, InventoryService inventoryService, BillingService billingService)
    {
        _context = context;
        _inventoryService = inventoryService;
        _billingService = billingService;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsArchived)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<SaleTransaction> CreateTransactionAsync(string userId, List<SaleItemDto> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var sale = new SaleTransaction
            {
                UserId = userId,
                TransactionDate = DateTime.UtcNow,
                SaleItems = new List<SaleItem>()
            };

            decimal totalAmount = 0;

            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");

                // Note: We don't update stock directly here anymore; InventoryService does it
                // product.StockQuantity -= item.Quantity;

                var saleItem = new SaleItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                totalAmount += saleItem.Quantity * saleItem.UnitPrice;
                sale.SaleItems.Add(saleItem);
            }

            sale.TotalAmount = totalAmount;

            _context.SaleTransactions.Add(sale);
            await _context.SaveChangesAsync(); // Save first to get sale.Id

            // Update inventory and log transaction for each item
            foreach (var item in sale.SaleItems)
            {
                await _inventoryService.UpdateStockAsync(
                    item.ProductId, 
                    -item.Quantity, 
                    "Sale", 
                    userId, 
                    sale.Id, 
                    $"Sale Transaction #{sale.Id}"
                );
            }
            await _context.SaveChangesAsync(); // Save inventory changes

            // Invoice creation handled in SalesController to include cashier-entered cash

            await transaction.CommitAsync();

            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<SaleTransaction>> GetTransactionHistoryAsync()
    {
        return await _context.SaleTransactions
            .Include(t => t.User)
            .Include(t => t.SaleItems)
            .ThenInclude(i => i.Product)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<(List<SaleTransaction> items, int totalCount)> GetTransactionHistoryPageAsync(int page, int pageSize)
    {
        var query = _context.SaleTransactions
            .Include(t => t.User)
            .Include(t => t.SaleItems)
            .ThenInclude(i => i.Product)
            .OrderByDescending(t => t.TransactionDate)
            .AsQueryable();

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }
}

using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Entities;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class PurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly InventoryService _inventoryService;

    public PurchaseService(ApplicationDbContext context, InventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;
    }

    public async Task<List<Supplier>> GetAllSuppliersAsync()
    {
        return await _context.Suppliers.ToListAsync();
    }

    public async Task<PurchaseTransaction> CreatePurchaseAsync(string userId, int supplierId, List<PurchaseItemDto> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var supplier = await _context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                throw new Exception("Supplier not found.");

            var purchase = new PurchaseTransaction
            {
                UserId = userId,
                SupplierId = supplierId,
                TransactionDate = DateTime.UtcNow,
                PurchaseItems = new List<PurchaseItem>()
            };

            decimal totalCost = 0;

            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {item.ProductId} not found.");

                // Note: Inventory update is now handled via InventoryService after saving the purchase
                // product.StockQuantity += item.Quantity;

                var purchaseItem = new PurchaseItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost
                };

                totalCost += purchaseItem.Quantity * purchaseItem.UnitCost;
                purchase.PurchaseItems.Add(purchaseItem);
            }

            purchase.TotalCost = totalCost;

            _context.PurchaseTransactions.Add(purchase);
            await _context.SaveChangesAsync(); // Save first to get ID

            // Update inventory and log transaction for each item
            foreach (var item in purchase.PurchaseItems)
            {
                await _inventoryService.UpdateStockAsync(
                    item.ProductId, 
                    item.Quantity, 
                    "Purchase", 
                    userId, 
                    purchase.Id, 
                    $"Purchase Transaction #{purchase.Id}"
                );
            }
            await _context.SaveChangesAsync(); // Save inventory changes

            await transaction.CommitAsync();

            return purchase;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<PurchaseTransaction>> GetPurchaseHistoryAsync()
    {
        return await _context.PurchaseTransactions
            .Include(t => t.User)
            .Include(t => t.Supplier)
            .Include(t => t.PurchaseItems)
            .ThenInclude(i => i.Product)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<(List<PurchaseTransaction> items, int totalCount)> GetPurchaseHistoryPageAsync(int page, int pageSize)
    {
        var query = _context.PurchaseTransactions
            .Include(t => t.User)
            .Include(t => t.Supplier)
            .Include(t => t.PurchaseItems)
            .ThenInclude(i => i.Product)
            .OrderByDescending(t => t.TransactionDate)
            .AsQueryable();
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<PurchaseTransaction?> GetPurchaseDetailsAsync(int id)
    {
        return await _context.PurchaseTransactions
            .Include(t => t.User)
            .Include(t => t.Supplier)
            .Include(t => t.PurchaseItems)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}

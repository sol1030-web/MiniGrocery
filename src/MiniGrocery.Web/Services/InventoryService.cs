using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Entities;
using MiniGrocery.Web.Models;

namespace MiniGrocery.Web.Services;

public class InventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductInventoryViewModel>> GetInventorySummaryAsync()
    {
        return await _context.Products
            .Where(p => !p.IsArchived)
            .Select(p => new ProductInventoryViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Category = p.Category,
                CurrentStock = p.StockQuantity,
                UnitPrice = p.Price,
                IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= 10
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public async Task<List<InventoryTransactionViewModel>> GetHistoryAsync()
    {
        return await _context.InventoryTransactions
            .Include(t => t.Product)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new InventoryTransactionViewModel
            {
                Id = t.Id,
                ProductName = t.Product.Name,
                Date = t.TransactionDate,
                Type = t.TransactionType,
                QuantityChange = t.QuantityChange,
                ReferenceId = t.ReferenceId,
                Note = t.Note,
                ByUserId = t.CreatedByUserId
            })
            .Take(100)
            .ToListAsync();
    }

    public async Task<(List<InventoryTransactionViewModel> items, int totalCount)> GetHistoryPageAsync(int page, int pageSize)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Product)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new InventoryTransactionViewModel
            {
                Id = t.Id,
                ProductName = t.Product.Name,
                Date = t.TransactionDate,
                Type = t.TransactionType,
                QuantityChange = t.QuantityChange,
                ReferenceId = t.ReferenceId,
                Note = t.Note,
                ByUserId = t.CreatedByUserId
            });
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    // Centralized method to update stock and log transaction
    public async Task UpdateStockAsync(int productId, int quantityChange, string type, string userId, int? referenceId = null, string note = "")
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new Exception($"Product {productId} not found");

        // Auto-unarchive on positive stock movement (e.g., purchase/restock)
        if (quantityChange > 0 && product.IsArchived)
        {
            product.IsArchived = false;
        }

        // Update actual stock
        product.StockQuantity += quantityChange;

        // Log transaction
        var transaction = new InventoryTransaction
        {
            ProductId = productId,
            QuantityChange = quantityChange,
            TransactionType = type,
            TransactionDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            ReferenceId = referenceId,
            Note = note
        };

        _context.InventoryTransactions.Add(transaction);
        // Note: SaveChanges is intentionally NOT called here to allow this to be part of a larger transaction scope
        // The caller must call SaveChangesAsync on the context
    }

    public async Task SetStockQuantityAsync(int productId, int newQuantity, string userId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new Exception($"Product {productId} not found");
        var delta = newQuantity - product.StockQuantity;
        if (delta == 0) return;
        await UpdateStockAsync(productId, delta, "Adjustment", userId, note: $"Manual adjustment to {newQuantity}");
        await _context.SaveChangesAsync();
    }

    public async Task ArchiveProductAsync(int productId, string userId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new Exception($"Product {productId} not found");
        if (product.IsArchived) return;

        // Log and zero out stock
        if (product.StockQuantity != 0)
        {
            await UpdateStockAsync(productId, -product.StockQuantity, "Archive", userId, note: "Archived product");
        }
        product.IsArchived = true;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductPriceAsync(int productId, decimal newPrice, string userId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new Exception($"Product {productId} not found");
        if (product.Price == newPrice) return;

        product.Price = newPrice;
        var transaction = new InventoryTransaction
        {
            ProductId = productId,
            QuantityChange = 0,
            TransactionType = "Reprice",
            TransactionDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            ReferenceId = null,
            Note = $"Unit price updated to {newPrice}"
        };
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<Product> CreateProductAsync(string name, string? category, decimal price, int initialStock, string? description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required", nameof(name));

        var normalizedName = name.Trim();
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.Name == normalizedName);
        if (existing != null)
        {
            if (existing.IsArchived)
            {
                existing.IsArchived = false;
                existing.Category = category;
                existing.Price = price;
                existing.Description = description;
                await _context.SaveChangesAsync();

                if (initialStock > 0)
                {
                    await UpdateStockAsync(existing.Id, initialStock, "Adjustment", createdBy, note: "Restore initial stock");
                    await _context.SaveChangesAsync();
                }

                return existing;
            }
            throw new InvalidOperationException("A product with the same name already exists.");
        }

        var product = new Product
        {
            Name = normalizedName,
            Category = category,
            Price = price,
            StockQuantity = 0,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        if (initialStock > 0)
        {
            await UpdateStockAsync(product.Id, initialStock, "Adjustment", createdBy, note: "Initial stock");
            await _context.SaveChangesAsync();
        }

        return product;
    }

    public async Task<List<ProductInventoryViewModel>> GetArchivedProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsArchived)
            .Select(p => new ProductInventoryViewModel
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Category = p.Category,
                CurrentStock = p.StockQuantity,
                UnitPrice = p.Price,
                IsLowStock = p.StockQuantity > 0 && p.StockQuantity <= 10
            })
            .OrderBy(p => p.ProductName)
            .ToListAsync();
    }

    public async Task RestoreProductAsync(int productId, string userId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) throw new Exception($"Product {productId} not found");
        if (!product.IsArchived) return;
        product.IsArchived = false;
        var transaction = new InventoryTransaction
        {
            ProductId = productId,
            QuantityChange = 0,
            TransactionType = "Unarchive",
            TransactionDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            ReferenceId = null,
            Note = "Product restored (unarchived)"
        };
        _context.InventoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
}

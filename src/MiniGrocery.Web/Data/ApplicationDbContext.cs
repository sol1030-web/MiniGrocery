using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<SaleTransaction> SaleTransactions { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }

    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
    public DbSet<PurchaseItem> PurchaseItems { get; set; }

    public DbSet<EmployeeDetails> EmployeeDetails { get; set; }
    public DbSet<PayrollTransaction> PayrollTransactions { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
    
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Billing relationships
        builder.Entity<Invoice>()
            .HasOne(i => i.SaleTransaction)
            .WithOne()
            .HasForeignKey<Invoice>(i => i.SaleTransactionId);

        builder.Entity<PaymentTransaction>()
            .HasOne(pt => pt.Invoice)
            .WithMany(i => i.PaymentTransactions)
            .HasForeignKey(pt => pt.InvoiceId);

        // Inventory relationships
        builder.Entity<InventoryTransaction>()
            .HasOne(it => it.Product)
            .WithMany()
            .HasForeignKey(it => it.ProductId);

        // Payroll relationships
        builder.Entity<EmployeeDetails>()
            .HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<EmployeeDetails>(e => e.UserId);

        builder.Entity<PayrollTransaction>()
            .HasOne(pt => pt.User)
            .WithMany()
            .HasForeignKey(pt => pt.UserId);

        // Additional configuration if needed
        builder.Entity<SaleTransaction>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId);

        builder.Entity<SaleItem>()
            .HasOne(si => si.SaleTransaction)
            .WithMany(st => st.SaleItems)
            .HasForeignKey(si => si.SaleTransactionId);

        builder.Entity<PurchaseTransaction>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);

        builder.Entity<PurchaseTransaction>()
            .HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId);
            
        builder.Entity<PurchaseItem>()
            .HasOne(pi => pi.PurchaseTransaction)
            .WithMany(pt => pt.PurchaseItems)
            .HasForeignKey(pi => pi.PurchaseTransactionId);
    }
}

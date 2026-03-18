using Microsoft.AspNetCore.Identity;
using MiniGrocery.Web.Entities;

namespace MiniGrocery.Web.Data;

public static class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        string[] roles = { "Manager", "Sales Staff", "Inventory Clerk", "System Administrator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

       
        var usersToSeed = new[]
        {
            new { Email = "admin@minigrocery.com", Role = "System Administrator", FullName = "System Admin", Position = "Administrator", Department = "IT" },
            new { Email = "manager@minigrocery.com", Role = "Manager", FullName = "Store Manager", Position = "Manager", Department = "Management" },
            new { Email = "sales@minigrocery.com", Role = "Sales Staff", FullName = "Sales Representative", Position = "Sales Associate", Department = "Sales" },
            new { Email = "inventory@minigrocery.com", Role = "Inventory Clerk", FullName = "Inventory Specialist", Position = "Clerk", Department = "Inventory" }
        };

        foreach (var userInfo in usersToSeed)
        {
            if (await userManager.FindByEmailAsync(userInfo.Email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true,
                    FullName = userInfo.FullName
                };

                var result = await userManager.CreateAsync(user, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userInfo.Role);

                    // Seed Employee Details
                    if (!context.EmployeeDetails.Any(e => e.UserId == user.Id))
                    {
                        context.EmployeeDetails.Add(new EmployeeDetails
                        {
                            UserId = user.Id,
                            Position = userInfo.Position,
                            Department = userInfo.Department,
                            BaseSalary = 3000.00m,
                            JoinDate = DateTime.UtcNow
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        // Seed Products
        if (!context.Products.Any())
        {
            var products = new Product[]
            {
                new Product{Name="Apple", Price=0.50m, StockQuantity=100, Category="Fruits"},
                new Product{Name="Banana", Price=0.30m, StockQuantity=150, Category="Fruits"},
                new Product{Name="Milk", Price=2.50m, StockQuantity=50, Category="Dairy"},
                new Product{Name="Bread", Price=1.50m, StockQuantity=60, Category="Bakery"},
                new Product{Name="Eggs (Dozen)", Price=3.00m, StockQuantity=40, Category="Dairy"},
                new Product{Name="Soda Can", Price=1.00m, StockQuantity=200, Category="Beverages"},
                new Product{Name="Chips", Price=1.25m, StockQuantity=100, Category="Snacks"},
                new Product{Name="Water Bottle", Price=0.75m, StockQuantity=300, Category="Beverages"},
            };

            foreach (var p in products)
            {
                context.Products.Add(p);
            }
            await context.SaveChangesAsync();
        }

        // Seed Suppliers
        if (!context.Suppliers.Any())
        {
            var suppliers = new Supplier[]
            {
                new Supplier { Name = "Fresh Farms Ltd", ContactEmail = "orders@freshfarms.com", ContactPhone = "555-0101" },
                new Supplier { Name = "Dairy Best Co.", ContactEmail = "sales@dairybest.com", ContactPhone = "555-0102" },
                new Supplier { Name = "Global Beverages Inc.", ContactEmail = "contact@globalbev.com", ContactPhone = "555-0103" },
                new Supplier { Name = "Bakery Wholesalers", ContactEmail = "orders@bakeryws.com", ContactPhone = "555-0104" },
            };

            foreach (var s in suppliers)
            {
                context.Suppliers.Add(s);
            }
            await context.SaveChangesAsync();
        }
    }
}

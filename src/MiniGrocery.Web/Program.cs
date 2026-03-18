using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MiniGrocery.Web.Data;
using MiniGrocery.Web.Services;
using MiniGrocery.Web.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<SalesService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<PayrollService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IExchangeRatesService, ExchangeRatesService>();
builder.Services.AddHttpClient<IHolidayService, HolidayService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
var defaultCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-PH");
defaultCulture.Culture.NumberFormat.CurrencySymbol = "₱";
defaultCulture.UICulture.NumberFormat.CurrencySymbol = "₱";

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = defaultCulture,
    SupportedCultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en-PH") },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { new System.Globalization.CultureInfo("en-PH") }
};
app.UseRequestLocalization(localizationOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

app.Run();

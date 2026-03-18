using Microsoft.AspNetCore.Mvc;

namespace prototype.Controllers;

public class ClientDashboardController : Controller
{
    public IActionResult Index()
    {
        ViewData["Title"] = "Client Dashboard";
        return View();
    }

    public IActionResult Orders()
    {
        ViewData["Title"] = "My Orders";
        return View();
    }

    public IActionResult Profile()
    {
        ViewData["Title"] = "My Profile";
        return View();
    }
}

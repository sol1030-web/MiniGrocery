using Microsoft.AspNetCore.Mvc;

namespace prototype.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        ViewData["Title"] = "Login";
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        // Simple bypass as requested: any login works
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        return RedirectToAction("Login");
    }
}

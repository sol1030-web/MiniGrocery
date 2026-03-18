using Microsoft.AspNetCore.Identity;

namespace MiniGrocery.Web.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}

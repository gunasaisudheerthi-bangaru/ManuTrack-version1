using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ManuTrackAPI.Pages.Dashboard;

public class IndexModel : PageModel
{
    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        // Redirect to login if not logged in
        if (HttpContext.Session.GetString("token") == null)
            return RedirectToPage("/Auth/Login");

        Role = HttpContext.Session.GetString("role") ?? "";
        Name = HttpContext.Session.GetString("name") ?? "";
        return Page();
    }
}
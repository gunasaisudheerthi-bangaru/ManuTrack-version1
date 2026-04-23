using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly AuthService _auth;

    public LoginModel(AuthService auth)
    {
        _auth = auth;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetString("token") != null)
            return RedirectToPage("/Dashboard/Index");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please enter email and password.";
            return Page();
        }

        var result = await _auth.LoginAsync(
            new LoginRequest(Email, Password));

        if (result == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        HttpContext.Session.SetString("token", result.Token);
        HttpContext.Session.SetString("role", result.Role);
        HttpContext.Session.SetString("name", result.Name);
        HttpContext.Session.SetString("userId", result.UserId.ToString());

        return RedirectToPage("/Dashboard/Index");
    }
}
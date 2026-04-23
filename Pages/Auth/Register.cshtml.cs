using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Services.Interfaces;
using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _auth;

    public RegisterModel(IAuthService auth)
    {
        _auth = auth;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet()
    {
        if (HttpContext.Session.GetString("token") != null)
            return RedirectToPage("/Dashboard/Index");

        if (await _auth.AdminExistsAsync())
            ErrorMessage = "Admin account already exists. Registration is closed. Please sign in with your Admin credentials.";

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Email) ||
            string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please fill all fields.";
            return Page();
        }

        var (user, error) = await _auth.RegisterAsync(
            new CreateUserRequest(Name, "Admin", Email, Phone, Password));

        if (error != null)
        {
            ErrorMessage = error;
            return Page();
        }

        SuccessMessage = "Admin registered successfully!";
        return Page();
    }
}
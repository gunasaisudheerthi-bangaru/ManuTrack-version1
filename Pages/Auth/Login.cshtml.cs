using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Models.DTOs;
using System.Text.RegularExpressions;

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
    public string EmailError { get; set; } = string.Empty;    // ← ADD THIS
    public string PasswordError { get; set; } = string.Empty; // ← ADD THIS
    public string InfoMessage { get; set; } = string.Empty;


    public IActionResult OnGet(string? message)
    {
        if (HttpContext.Session.GetString("token") != null)
            return RedirectToPage("/Dashboard/Index");

        if (!string.IsNullOrEmpty(message))
            InfoMessage = message;


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate Email
        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = "Email is required.";
        }
        else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            EmailError = "Please enter a valid email address.";
        }

        // Validate Password
        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Password is required.";
        }
        else if (Password.Length < 6)
        {
            PasswordError = "Password must be at least 6 characters.";
        }

        // If validation fails → return page with errors
        if (!string.IsNullOrEmpty(EmailError) ||
            !string.IsNullOrEmpty(PasswordError))
        {
            return Page();
        }

        // Call service
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

        return RedirectToPage("/Dashboard/Index");
    }
}
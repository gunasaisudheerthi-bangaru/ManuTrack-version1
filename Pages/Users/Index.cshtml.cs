using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Models;
using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Pages.Users;

public class IndexModel : PageModel
{
    private readonly AuthService _auth;

    public IndexModel(AuthService auth)
    {
        _auth = auth;
    }

    public List<UserResponse> Users { get; set; } = new();
    public string SuccessMessage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("token") == null)
            return RedirectToPage("/Auth/Login");

        if (HttpContext.Session.GetString("role") != "Admin")
            return RedirectToPage("/Dashboard/Index");

        Users = await _auth.GetAllUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(
     string Name, string Email, string Phone,
     string Role, string Password)
    {
        // Validate Name
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        // Validate Email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Email is required.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        var emailRegex = new System.Text.RegularExpressions
            .Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(Email))
        {
            ErrorMessage = "Please enter a valid email address.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        // Validate Phone
        if (string.IsNullOrWhiteSpace(Phone))
        {
            ErrorMessage = "Phone number is required.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        var phoneRegex = new System.Text.RegularExpressions
            .Regex(@"^[0-9]{10}$");
        if (!phoneRegex.IsMatch(Phone))
        {
            ErrorMessage = "Phone number must be exactly 10 digits.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        // Validate Password
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        // All validation passed → create user
        var (user, error) = await _auth.CreateUserAsync(
            new CreateUserRequest(Name, Role, Email, Phone, Password),
            GetActorId());

        if (error != null)
            ErrorMessage = error;
        else
            SuccessMessage = $"User {Name} created successfully!";

        Users = await _auth.GetAllUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(int userId)
    {
        await _auth.DeactivateUserAsync(userId, GetActorId());
        SuccessMessage = "User deactivated.";
        Users = await _auth.GetAllUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostActivateAsync(int userId)
    {
        var user = await _auth.GetUserByIdAsync(userId);
        if (user == null)
        {
            ErrorMessage = "User not found.";
            Users = await _auth.GetAllUsersAsync();
            return Page();
        }

        await _auth.UpdateUserAsync(userId, new UpdateUserRequest(
            user.Name,
            user.Role,
            user.Phone,
            true   // ← IsActive = true
        ), GetActorId());

        SuccessMessage = $"User {user.Name} activated successfully!";
        Users = await _auth.GetAllUsersAsync();
        return Page();
    }

    private int GetActorId()
    {
        var token = HttpContext.Session.GetString("token");
        if (token == null) return 0;

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var id = jwt.Claims.FirstOrDefault(c =>
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?.Value;
        return int.TryParse(id, out var result) ? result : 0;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Models.DTOs;
using ManuTrackAPI.Models;

namespace ManuTrackAPI.Pages.Profile;

public class IndexModel : PageModel
{
    private readonly AuthService _auth;

    public IndexModel(AuthService auth)
    {
        _auth = auth;
    }

    public UserResponse? CurrentUser { get; set; }
    public List<AuditLog> RecentActivity { get; set; } = new();
    public string SuccessMessage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    [BindProperty]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("token") == null)
            return RedirectToPage("/Auth/Login");

        var userId = GetUserId();
        CurrentUser = await _auth.GetUserByIdAsync(userId);

        var allLogs = await _auth.GetAuditLogsAsync(userId);
        RecentActivity = allLogs.Take(5).ToList();

        Phone = CurrentUser?.Phone ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdatePhoneAsync()
    {
        var userId = GetUserId();

        var (success, error) = await _auth.UpdateProfileAsync(
            userId, new UpdateProfileRequest(Phone));

        if (!success)
            ErrorMessage = error ?? "Could not update profile.";
        else
            SuccessMessage = "Phone number updated successfully.";

        await ReloadAsync(userId);
        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var userId = GetUserId();

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "New passwords do not match.";
            await ReloadAsync(userId);
            return Page();
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            await ReloadAsync(userId);
            return Page();
        }

        var (success, error) = await _auth.ChangePasswordAsync(
            userId, new ChangePasswordRequest(CurrentPassword, NewPassword));

        if (!success)
            ErrorMessage = error ?? "Could not change password.";
        else
            SuccessMessage = "Password changed successfully.";

        await ReloadAsync(userId);
        return Page();
    }

    private async Task ReloadAsync(int userId)
    {
        CurrentUser = await _auth.GetUserByIdAsync(userId);
        var allLogs = await _auth.GetAuditLogsAsync(userId);
        RecentActivity = allLogs.Take(5).ToList();
        Phone = CurrentUser?.Phone ?? string.Empty;
    }

    private int GetUserId()
    {
        var userIdStr = HttpContext.Session.GetString("userId");
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }
}
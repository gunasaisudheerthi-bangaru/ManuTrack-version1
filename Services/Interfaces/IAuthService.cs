using ManuTrackAPI.Models;
using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Services.Interfaces;

public interface IAuthService
{
    Task<(UserResponse? user, string? error)> RegisterAsync(CreateUserRequest req);

    Task<LoginResponse?> LoginAsync(LoginRequest req);

    Task<bool> AdminExistsAsync();

    Task<(UserResponse? user, string? error)> CreateUserAsync(
    CreateUserRequest req, int actorId);

    Task<List<UserResponse>> GetAllUsersAsync();

    Task<UserResponse?> GetUserByIdAsync(int id);

    Task<(UserResponse? user, string? error)> UpdateUserAsync(
        int id, UpdateUserRequest req, int actorId);

    Task<(bool success, string? error)> ChangePasswordAsync(
        int userId, ChangePasswordRequest req);

    Task<bool> DeactivateUserAsync(int id, int actorId);

    Task<List<AuditLog>> GetAuditLogsAsync(int? userId = null);

    Task<List<UserResponse>> GetOperatorsAsync();

   


    Task WriteAuditAsync(int userId, string action, string? details = null);
}
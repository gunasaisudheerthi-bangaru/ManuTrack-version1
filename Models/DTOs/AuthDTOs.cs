namespace ManuTrackAPI.Models.DTOs;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Role, string Name);
public record CreateUserRequest(string Name, string Role, string Email, string Phone, string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateUserRequest(string Name, string Role, string Phone, bool IsActive);
public record UserResponse(int UserID, string Name, string Role, string Email, string Phone, bool IsActive, DateTime CreatedAt);












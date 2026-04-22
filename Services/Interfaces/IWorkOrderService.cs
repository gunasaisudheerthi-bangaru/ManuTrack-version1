using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Services.Interfaces;

public interface IWorkOrderService
{
    // Work Orders
    Task<List<WorkOrderResponse>> GetAllAsync();

    Task<WorkOrderResponse?> GetByIdAsync(int id);

    Task<(WorkOrderResponse? wo, string? error)> CreateAsync(
        CreateWorkOrderRequest req, int actorId);

    Task<(WorkOrderResponse? wo, string? error)> UpdateStatusAsync(
        int id, string status, int actorId);

    Task<(bool success, string? error)> CancelAsync(
        int id, int actorId);

    // Tasks
    Task<List<TaskResponse>> GetTasksByWorkOrderAsync(int workOrderId);

    Task<(TaskResponse? task, string? error)> CreateTaskAsync(
        int workOrderId, CreateTaskRequest req, int actorId);

    Task<(TaskResponse? task, string? error)> UpdateTaskStatusAsync(
        int taskId, string status, int actorId);
}
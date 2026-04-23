using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services;
using ManuTrackAPI.Services.Interfaces;
using ManuTrackAPI.Models.DTOs;

namespace ManuTrackAPI.Pages.WorkOrders;

public class IndexModel : PageModel
{
    private readonly IWorkOrderService _workOrders;
    private readonly IProductService _products;
    private readonly IAuthService _auth;

    public IndexModel(IWorkOrderService workOrders,
        IProductService products, IAuthService auth)
    {
        _workOrders = workOrders;
        _products = products;
        _auth = auth;
    }

    public string Role { get; set; } = string.Empty;
    public int CurrentUserId { get; set; }
    public string SuccessMessage { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public List<WorkOrderResponse> WorkOrders { get; set; } = new();
    public List<ProductResponse> Products { get; set; } = new();
    public List<UserResponse> Operators { get; set; } = new();
    public List<TaskResponse> Tasks { get; set; } = new();
    public WorkOrderResponse? SelectedWO { get; set; }

    public async Task<IActionResult> OnGetAsync(int? woId)
    {
        if (HttpContext.Session.GetString("token") == null)
            return RedirectToPage("/Auth/Login");

        Role = HttpContext.Session.GetString("role") ?? "";
        CurrentUserId = GetActorId();

        await LoadDataAsync();

        if (woId.HasValue)
        {
            SelectedWO = WorkOrders.FirstOrDefault(
                w => w.WorkOrderID == woId.Value);
            if (SelectedWO != null)
                Tasks = await _workOrders.GetTasksByWorkOrderAsync(
                    SelectedWO.WorkOrderID);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(
        int ProductID, int Quantity,
        DateTime StartDate, DateTime EndDate)
    {
        Role = HttpContext.Session.GetString("role") ?? "";

        if (Role != "Planner")
        {
            ErrorMessage = "Only Planners can create work orders.";
            await LoadDataAsync();
            return Page();
        }

        var (wo, error) = await _workOrders.CreateAsync(
            new CreateWorkOrderRequest(ProductID, Quantity, StartDate, EndDate),
            GetActorId());

        if (error != null)
            ErrorMessage = error;
        else
            SuccessMessage = $"Work Order #{wo!.WorkOrderID} created!";

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(
        int woId, string status)
    {
        Role = HttpContext.Session.GetString("role") ?? "";

        if (Role != "Planner")
        {
            ErrorMessage = "Only Planners can update work order status.";
            await LoadDataAsync();
            SelectedWO = WorkOrders.FirstOrDefault(w => w.WorkOrderID == woId);
            if (SelectedWO != null)
                Tasks = await _workOrders.GetTasksByWorkOrderAsync(woId);
            return Page();
        }

        var (wo, error) = await _workOrders.UpdateStatusAsync(
            woId, status, GetActorId());

        if (error != null)
            ErrorMessage = error;
        else
        {
            SuccessMessage = $"Status updated to {status}";
            SelectedWO = wo;
            Tasks = await _workOrders.GetTasksByWorkOrderAsync(woId);
        }

        await LoadDataAsync();
        return RedirectToPage(new { woId });
    }

    public async Task<IActionResult> OnPostCancelAsync(int woId)
    {
        Role = HttpContext.Session.GetString("role") ?? "";

        if (Role != "Planner")
        {
            ErrorMessage = "Only Planners can cancel work orders.";
            await LoadDataAsync();
            return Page();
        }

        var (success, error) = await _workOrders.CancelAsync(
            woId, GetActorId());

        if (!success)
            ErrorMessage = error ?? "Failed to cancel.";
        else
            SuccessMessage = "Work order cancelled.";

        return RedirectToPage(new { woId });
    }

    public async Task<IActionResult> OnPostAddTaskAsync(
        int woId, string Description, int AssignedTo)
    {
        Role = HttpContext.Session.GetString("role") ?? "";

        if (Role != "Planner")
        {
            ErrorMessage = "Only Planners can add tasks.";
            await LoadDataAsync();
            return Page();
        }

        var (task, error) = await _workOrders.CreateTaskAsync(
            woId,
            new CreateTaskRequest(Description, AssignedTo),
            GetActorId());

        if (error != null)
            ErrorMessage = error;
        else
            SuccessMessage = "Task added successfully!";

        return RedirectToPage(new { woId });
    }

    public async Task<IActionResult> OnPostUpdateTaskStatusAsync(
        int taskId, int woId, string status)
    {
        Role = HttpContext.Session.GetString("role") ?? "";
        var actorId = GetActorId();

        if (Role != "Operator")
        {
            ErrorMessage = "Only Operators can update task status.";
            await LoadDataAsync();
            SelectedWO = WorkOrders.FirstOrDefault(w => w.WorkOrderID == woId);
            if (SelectedWO != null)
                Tasks = await _workOrders.GetTasksByWorkOrderAsync(woId);
            return Page();
        }

        // Operator can only update tasks assigned to them
        var allTasks = await _workOrders.GetTasksByWorkOrderAsync(woId);
        var task = allTasks.FirstOrDefault(t => t.TaskID == taskId);

        if (task == null || task.AssignedTo != actorId)
        {
            ErrorMessage = "You can only update tasks assigned to you.";
            await LoadDataAsync();
            SelectedWO = WorkOrders.FirstOrDefault(w => w.WorkOrderID == woId);
            Tasks = allTasks;
            return RedirectToPage(new { woId });
        }

        var (updatedTask, error) = await _workOrders.UpdateTaskStatusAsync(
            taskId, status, actorId);

        if (error != null)
            ErrorMessage = error;
        else
            SuccessMessage = $"Task marked as {status}";

        return RedirectToPage(new { woId });
    }

    private async Task LoadDataAsync()
    {
        WorkOrders = (await _workOrders.GetAllAsync())
                        .OrderByDescending(w => w.WorkOrderID)
                        .ToList();
        Products = await _products.GetAllProductsAsync();
        Operators = await _auth.GetOperatorsAsync();
    }

    private int GetActorId()
    {
        var token = HttpContext.Session.GetString("token");
        if (token == null) return 0;

        var handler = new System.IdentityModel.Tokens.Jwt
            .JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var id = jwt.Claims.FirstOrDefault(c =>
            c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/" +
            "claims/nameidentifier")?.Value;
        return int.TryParse(id, out var result) ? result : 0;
    }
}
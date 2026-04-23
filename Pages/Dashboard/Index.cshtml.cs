using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ManuTrackAPI.Services.Interfaces;
using ManuTrackAPI.Models.DTOs;
using ManuTrackAPI.Models;

namespace ManuTrackAPI.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IAuthService _auth;
    private readonly IWorkOrderService _workOrders;
    private readonly IProductService _products;

    public IndexModel(IAuthService auth, IWorkOrderService workOrders, IProductService products)
    {
        _auth = auth;
        _workOrders = workOrders;
        _products = products;
    }

    public string Role { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // ── ADMIN ──────────────────────────────────────────────────
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int DraftProducts { get; set; }
    public int TotalWorkOrders { get; set; }
    public int WOPendingAdmin { get; set; }
    public int WOInProgressAdmin { get; set; }
    public int WOCompletedAdmin { get; set; }
    public List<AuditLog> RecentAuditLogs { get; set; } = new();

    // ── PLANNER ────────────────────────────────────────────────
    public int WOPending { get; set; }
    public int WOInProgress { get; set; }
    public int WOCompleted { get; set; }
    public int WOCancelled { get; set; }
    public int WOOverdue { get; set; }
    public List<ProductResponse> ProductsNoBOM { get; set; } = new();
    public List<WorkOrderResponse> RecentWorkOrders { get; set; } = new();

    // ── OPERATOR ───────────────────────────────────────────────
    public int TaskPending { get; set; }
    public int TaskInProgress { get; set; }
    public int TaskDone { get; set; }
    public List<TaskResponse> MyTasks { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.Session.GetString("token") == null)
            return RedirectToPage("/Auth/Login");

        Role = HttpContext.Session.GetString("role") ?? "";
        Name = HttpContext.Session.GetString("name") ?? "";

        if (Role == "Admin")
            await LoadAdminDataAsync();
        else if (Role == "Planner")
            await LoadPlannerDataAsync();
        else if (Role == "Operator")
            await LoadOperatorDataAsync();

        return Page();
    }

    private async Task LoadAdminDataAsync()
    {
        var users = await _auth.GetAllUsersAsync();
        TotalUsers = users.Count;

        var products = await _products.GetAllProductsAsync();
        TotalProducts = products.Count;
        ActiveProducts = products.Count(p => p.Status == "Active");
        DraftProducts = products.Count(p => p.Status == "Draft");

        var workOrders = await _workOrders.GetAllAsync();
        TotalWorkOrders = workOrders.Count;
        WOPendingAdmin = workOrders.Count(w => w.Status == "Pending");
        WOInProgressAdmin = workOrders.Count(w => w.Status == "InProgress");
        WOCompletedAdmin = workOrders.Count(w => w.Status == "Completed");

        RecentAuditLogs = (await _auth.GetAuditLogsAsync()).Take(8).ToList();
    }

    private async Task LoadPlannerDataAsync()
    {
        var allWOs = await _workOrders.GetAllAsync();
        WOPending = allWOs.Count(w => w.Status == "Pending");
        WOInProgress = allWOs.Count(w => w.Status == "InProgress");
        WOCompleted = allWOs.Count(w => w.Status == "Completed");
        WOCancelled = allWOs.Count(w => w.Status == "Cancelled");
        WOOverdue = allWOs.Count(w =>
            w.Status != "Completed" &&
            w.Status != "Cancelled" &&
            w.EndDate < DateTime.Now);


        // Products with no active BOMs
        var allProducts = await _products.GetAllProductsAsync();
        var productsNoBOM = new List<ProductResponse>();
        foreach (var product in allProducts.Where(p => p.Status == "Active"))
        {
            var boms = await _products.GetBOMsByProductAsync(product.ProductID);
            if (!boms.Any(b => b.Status == "Active"))
                productsNoBOM.Add(product);
        }
        ProductsNoBOM = productsNoBOM;

        RecentWorkOrders = allWOs
            .OrderByDescending(w => w.WorkOrderID)
            .Take(5)
            .ToList();
    }

    private async Task LoadOperatorDataAsync()
    {
        var userId = GetUserId();
        var allWOs = await _workOrders.GetAllAsync();
        var myTasks = new List<TaskResponse>();

        foreach (var wo in allWOs)
        {
            var tasks = await _workOrders.GetTasksByWorkOrderAsync(wo.WorkOrderID);
            myTasks.AddRange(tasks.Where(t => t.AssignedTo == userId));
        }

        TaskPending = myTasks.Count(t => t.Status == "Pending");
        TaskInProgress = myTasks.Count(t => t.Status == "InProgress");
        TaskDone = myTasks.Count(t => t.Status == "Done");
        MyTasks = myTasks
            .OrderBy(t => t.Status == "Done")
            .Take(6)
            .ToList();
    }

    private int GetUserId()
    {
        var userIdStr = HttpContext.Session.GetString("userId");
        return int.TryParse(userIdStr, out var id) ? id : 0;
    }
}
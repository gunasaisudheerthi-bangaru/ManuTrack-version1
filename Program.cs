using ManuTrackAPI.Data;
using ManuTrackAPI.Services;
using ManuTrackAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
});

// Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/Auth/Login"));

app.UseStaticFiles();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eCommerce.Web.Services;
using eCommerce.Web.Services.Notifications;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("VietTechConnection");
var dbProvider = builder.Configuration["DatabaseProvider"];

// === CẤU HÌNH IEmailSender ===
// Lấy thông tin cấu hình (nên đặt trong appsettings.json)
var smtpServer = builder.Configuration["EmailSettings:SmtpServer"];
// Port có thể không được cấu hình trong appsettings => sử dụng TryParse an toàn
var portStr = builder.Configuration["EmailSettings:Port"];
int port;
if (string.IsNullOrWhiteSpace(portStr) || !int.TryParse(portStr, out port))
{
    // Mặc định dùng 25 nếu không cấu hình hoặc cấu hình không hợp lệ
    port = 25;
    Console.WriteLine("Warning: EmailSettings:Port not configured or invalid. Falling back to default port 25.");
}
var fromEmail = builder.Configuration["EmailSettings:FromEmail"];
var appPassword = builder.Configuration["EmailSettings:AppPassword"]; // Lấy Mật khẩu ứng dụng

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider == "SQLServer")
    {
        options.UseSqlServer(connectionString);
        Console.WriteLine("--> Using SQL Server DB");
    }
    else
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        Console.WriteLine("--> Using MySQL DB");
    }
});

// Đăng ký dịch vụ EmailSender
builder.Services.AddSingleton<IEmailSender>(new EmailSender(smtpServer, port, fromEmail, appPassword));

// Thêm vào phần đăng ký services
builder.Services.AddScoped<IVnPayService, VnPayService>();
// CẬP NHẬT: Thêm .AddRoles<IdentityRole>()
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Thêm dòng này
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();

// Đăng ký các Repository và Service
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<eCommerce.Core.Interfaces.IOrderRepository, eCommerce.Infrastructure.Data.OrderRepository>();

// === ĐĂNG KÝ MỚI ===
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
// === HẾT ĐĂNG KÝ MỚI ===

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<eCommerce.Application.Services.IOrderService, eCommerce.Application.Services.OrderService>();

// Push service (Web Push)
builder.Services.AddScoped<eCommerce.Web.Services.IPushService, eCommerce.Web.Services.WebPushService>();

// Notification queue + background worker
builder.Services.AddSingleton<INotificationQueue, NotificationQueue>();
builder.Services.AddHostedService<NotificationBackgroundService>();

// Add cart, voucher and session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

// Add loyalty service
builder.Services.AddScoped<eCommerce.Application.Services.ILoyaltyService, eCommerce.Infrastructure.Services.LoyaltyService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// === CẬP NHẬT PHẦN SEED DATA ===
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // Lấy UserManager và RoleManager
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Đảm bảo CSDL được tạo (Identity tables, etc.)
        // Chúng ta sẽ dùng migration để cập nhật, nhưng EnsureCreated() an toàn cho lần chạy đầu


        // Truyền các dịch vụ vào DbInitializer
        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// === HẾT CẬP NHẬT SEED DATA ===

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
using eCommerce.Application.Services;
using eCommerce.Application.Strategies.Payment;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eCommerce.Web.Services;
using eCommerce.Web.Services.Notifications;
using Microsoft.Extensions.Caching.Memory;
using WebPush;

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
builder.Services.AddScoped<eCommerce.Web.Services.Notifications.IPushService, eCommerce.Web.Services.Notifications.WebPushService>();

// Notification queue + background worker
builder.Services.AddSingleton<INotificationQueue, NotificationQueue>();
builder.Services.AddHostedService<NotificationBackgroundService>();
builder.Services.AddScoped<IPushService, WebPushService>();


// Add cart, voucher and session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

// ============================
//  FACADE PATTERN
//  Đăng ký CheckoutFacade — bọc 5 subsystems vào 1 giao diện đơn giản
//  (IOrderService, IStockService, ILoyaltyService, ICartService, AppDbContext)
// ============================
builder.Services.AddScoped<ICheckoutFacade, CheckoutFacade>();

// Add loyalty service
builder.Services.AddScoped<eCommerce.Application.Services.ILoyaltyService, eCommerce.Infrastructure.Services.LoyaltyService>();

// Add stock management service
builder.Services.AddScoped<IStockService, StockService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add SignalR for real-time chat
builder.Services.AddSignalR();
//----------------------Proxy-----------------------------------
// 1. Kích hoạt dịch vụ Memory Cache của ASP.NET Core
builder.Services.AddMemoryCache();
// 2. Đăng ký Service thật
builder.Services.AddScoped<ProductService>();

// 3. Đăng ký Proxy sử dụng Interface IProductService
// Khi Controller gọi IProductService, nó sẽ nhận được bản Proxy
builder.Services.AddScoped<IProductService>(provider =>
{
    var realService = provider.GetRequiredService<ProductService>();
    var cache = provider.GetRequiredService<IMemoryCache>();
    return new ProductServiceProxy(realService, cache);
});
//------------------------Proxy-----------------------------------------

//---------------------------Observer----------------------------------
// Đăng ký dịch vụ tích điểm làm Observer
builder.Services.AddScoped<IOrderObserver, eCommerce.Application.Observers.LoyaltyOrderObserver>();

// Cấu hình OrderService bằng Factory pattern để tự động kết nối với các Observers
builder.Services.AddScoped<IOrderService>(provider => 
{
    var orderRepository = provider.GetRequiredService<IOrderRepository>();
    var orderService = new eCommerce.Application.Services.OrderService(orderRepository);
    
    // Tìm tất cả các IOrderObserver đã được đăng ký và Attach vào Subject
    var observers = provider.GetServices<IOrderObserver>();
    foreach(var observer in observers)
    {
        orderService.Attach(observer);
    }
    
    return orderService;
});

// Đăng ký dịch vụ thông báo làm Observer thứ 2
builder.Services.AddScoped<IOrderObserver, eCommerce.Application.Observers.CustomerNotificationObserver>();
//---------------------------Observer----------------------------------

//---------------------------Strategy---------------------------------
// Đăng ký các chiến lược thanh toán
builder.Services.AddScoped<IPaymentStrategy, eCommerce.Application.Strategies.Payment.CodPaymentStrategy>();
builder.Services.AddScoped<IPaymentStrategy, eCommerce.Application.Strategies.Payment.VnPayPaymentStrategy>();

//---------------------------Strategy---------------------------------
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

        // Auto migrate database when in Production
        if (!app.Environment.IsDevelopment())
        {
            Console.WriteLine("--> Running database migrations...");
            context.Database.Migrate();
            Console.WriteLine("--> Migrations completed!");
        }

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

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();    
app.MapControllers();
app.MapHub<eCommerce.Web.Hubs.ChatHub>("/chatHub");

// --- BẮT ĐẦU ĐOẠN CODE TẠO KEY (CHẠY 1 LẦN RỒI XÓA) ---
/*
try 
{
    var keys = VapidHelper.GenerateVapidKeys();
    Console.WriteLine("\n=================================================");
    Console.WriteLine("COPY 2 DÒNG DƯỚI ĐÂY VÀO APPSETTINGS.JSON:");
    Console.WriteLine($"PublicKey:  {keys.PublicKey}");
    Console.WriteLine($"PrivateKey: {keys.PrivateKey}");
    Console.WriteLine("=================================================\n");
    
    // Dừng chương trình lại để bạn kịp copy, không chạy server web lên
    return; 
}
catch (Exception ex)
{
    Console.WriteLine("Lỗi: " + ex.Message);
} */
// --- KẾT THÚC ĐOẠN CODE TẠO KEY ---
app.Run();
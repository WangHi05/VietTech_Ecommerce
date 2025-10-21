using eCommerce.Application.Services;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// === PHẦN CẬP NHẬT ĐỂ HỖ TRỢ ĐA CSDL ===

// 1. Lấy chuỗi kết nối từ cấu hình (sẽ lấy từ appsettings.Development.json nếu có)
var connectionString = builder.Configuration.GetConnectionString("VietTechConnection");

// 2. Lấy tên nhà cung cấp CSDL
var dbProvider = builder.Configuration["DatabaseProvider"];

// 3. Đăng ký DbContext dựa trên nhà cung cấp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider == "SQLServer")
    {
        options.UseSqlServer(connectionString);
        Console.WriteLine("--> Using SQL Server DB");
    }
    else // Mặc định dùng MySQL
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        Console.WriteLine("--> Using MySQL DB");
    }
});

// === KẾT THÚC PHẦN CẬP NHẬT ===


// Đăng ký các services và repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Thêm dịch vụ cho Razor Pages
builder.Services.AddRazorPages();

// Giữ lại controllers cho API nếu bạn muốn dùng sau này
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Gọi phương thức seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Đảm bảo CSDL được tạo
        context.Database.EnsureCreated();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseHttpsRedirection();
// Cho phép phục vụ các file tĩnh (CSS, JS, Images) từ thư mục wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Ánh xạ các Razor Pages
app.MapRazorPages();
// Ánh xạ các API Controllers
app.MapControllers();

app.Run();
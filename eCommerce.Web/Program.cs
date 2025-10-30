using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using eCommerce.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("VietTechConnection");
var dbProvider = builder.Configuration["DatabaseProvider"];

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

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<eCommerce.Core.Interfaces.IOrderRepository, eCommerce.Infrastructure.Data.OrderRepository>();
builder.Services.AddScoped<eCommerce.Application.Services.IOrderService, eCommerce.Application.Services.OrderService>();

// Add cart, voucher and session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
    var context = services.GetRequiredService<AppDbContext>();
    // If migrations can't be applied cleanly in this environment, fall back to EnsureCreated
    // to create the schema for development/testing. This will create the necessary
    // tables (including Identity tables) when the DB is empty.
    // Ensure existing schema and create Orders tables if missing (safe for dev)
    context.Database.EnsureCreated();

    // Create Orders and OrderItems tables if they don't exist to avoid runtime errors
    // in environments where we can't scaffold/apply EF migrations.
    var createOrdersSql = @"
IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [ShippingName] NVARCHAR(200) NOT NULL,
        [ShippingAddress] NVARCHAR(500) NOT NULL,
        [ShippingCountry] NVARCHAR(100) NULL,
        [ShippingProvince] NVARCHAR(100) NULL,
        [SubTotal] DECIMAL(18,2) NOT NULL,
        [Discount] DECIMAL(18,2) NOT NULL,
        [ShippingFee] DECIMAL(18,2) NOT NULL,
        [VoucherCode] NVARCHAR(100) NULL,
        [Total] DECIMAL(18,2) NOT NULL
    );
END

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItems](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [Name] NVARCHAR(300) NULL,
        [Price] DECIMAL(18,2) NOT NULL,
        [Quantity] INT NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE
    );
END
";

    await context.Database.ExecuteSqlRawAsync(createOrdersSql);

        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

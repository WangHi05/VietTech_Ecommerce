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
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();

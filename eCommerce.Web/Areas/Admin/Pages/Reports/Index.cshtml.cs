using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data; // Cần dùng AppDbContext
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages.Reports
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context; // Dùng DbContext trực tiếp
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Các con số thống kê
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal RevenueToday { get; set; }
        public int OrdersToday { get; set; }
        
        // Danh sách đơn hàng mới nhất
        public List<Order> RecentOrders { get; set; } = new();

        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartData { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Lấy mốc thời gian hôm nay (bắt đầu từ 00:00)
            // Dùng .Date để lấy ngày, bỏ qua giờ
            var todayStart = DateTime.UtcNow.Date;

            // 1. Tổng doanh thu (chỉ tính đơn đã hoàn thành, nếu bạn có trạng thái)
            // (Ví dụ: chỉ tính đơn có Status == "Completed")
            // Nếu không, cứ Sum() tất cả:
            TotalRevenue = await _context.Orders
                // .Where(o => o.Status == "Completed") 
                .SumAsync(o => o.Total);

            // 2. Tổng số đơn hàng
            TotalOrders = await _context.Orders.CountAsync();

            // 3. Tổng số khách hàng
            TotalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count;

            // 4. Doanh thu hôm nay
            RevenueToday = await _context.Orders
                .Where(o => o.CreatedAt >= todayStart)
                .SumAsync(o => o.Total);
            
            // 5. Số đơn hàng hôm nay
            OrdersToday = await _context.Orders
                .CountAsync(o => o.CreatedAt >= todayStart);

            // 6. Lấy 5 đơn hàng gần đây nhất
            RecentOrders = await _context.Orders
                .Include(o => o.Customer) // Lấy kèm tên khách hàng
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();
            // 1. Lấy mốc 7 ngày trước (từ 00:00)
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);

            // 2. Truy vấn và nhóm doanh thu theo ngày
            var dailyRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= sevenDaysAgo) // Chỉ lấy 7 ngày qua
                .GroupBy(o => o.CreatedAt.Date) // Nhóm theo Ngày
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(o => o.Total)
                })
                .ToDictionaryAsync(r => r.Date, r => r.Total);

            // 3. Chuẩn bị dữ liệu cho biểu đồ
            // (Lặp qua 7 ngày để đảm bảo ngày nào không có doanh thu vẫn hiển thị là 0)
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                
                // Thêm Nhãn (ví dụ: "08/11")
                ChartLabels.Add(date.ToString("dd/MM"));

                // Thêm Dữ liệu (nếu ngày đó có doanh thu thì lấy, không thì 0)
                if (dailyRevenue.ContainsKey(date))
                {
                    ChartData.Add(dailyRevenue[date]);
                }
                else
                {
                    ChartData.Add(0);
                }
            }  
        }
    }
}
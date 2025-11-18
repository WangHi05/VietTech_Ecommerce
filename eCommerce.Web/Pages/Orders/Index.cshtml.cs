using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly eCommerce.Infrastructure.Data.AppDbContext _context;

        public List<Order> Orders { get; set; } = new();

        // orders that already have at least one review by this user
        public System.Collections.Generic.HashSet<int> ReviewedOrderIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FilterStatus { get; set; }

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize = 3; 
        public IndexModel(IOrderService orderService, eCommerce.Infrastructure.Data.AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                Orders = new List<Order>();
                return;
            }

            // 1. Lấy tất cả đơn hàng (giả định GetOrdersByUserAsync tải luôn Items)
            var allOrders = await _orderService.GetOrdersByUserAsync(userId);

            // 2. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(FilterStatus))
            {
                allOrders = allOrders.Where(o => o.Status == FilterStatus).ToList();
            }
            
            // 3. Sắp xếp (mới nhất lên trên)
            allOrders = allOrders.OrderByDescending(o => o.CreatedAt).ToList();

            // 4. Lấy thông tin đánh giá
            try
            {
                var ids = allOrders.Select(o => o.Id).ToList();
                var reviewed = await _context.Reviews
                    .Where(r => r.OrderId.HasValue && ids.Contains(r.OrderId.Value) && r.UserId == userId)
                    .Select(r => r.OrderId)
                    .Distinct()
                    .Where(o => o.HasValue)
                    .Select(o => o!.Value)
                    .ToListAsync();

                ReviewedOrderIds = new System.Collections.Generic.HashSet<int>(reviewed);
            }
            catch
            {
                // ignore DB errors; Orders list still usable
            }

            // 5. Áp dụng phân trang
            var count = allOrders.Count;
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            Orders = allOrders.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id); // Dùng service để lấy đơn hàng

            if (order == null)
            {
                return NotFound();
            }
            if (order.UserId != userId)
            {
                return Forbid();
            }

            // Chỉ cho phép hủy khi đang ở trạng thái "Pending"
            if (order.Status == "Đang chờ")
            {
                // Cập nhật trạng thái. "Cancelled" sẽ được dịch sang "Đã hủy" ở view
                order.Status = "Đã huỷ"; 
                
                // Lưu thay đổi vào DB
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hủy đơn hàng #" + id + " thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng #" + id + " ở trạng thái này.";
            }

            return RedirectToPage("/Orders/Index", new { FilterStatus = this.FilterStatus, PageIndex = this.PageIndex});
        }
        
        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id); // Dùng service để lấy đơn hàng

            if (order == null)
            {
                return NotFound();
            }
            if (order.UserId != userId)
            {
                return Forbid();
            }

            // Chỉ cho phép xác nhận khi đang "Đang Giao"
            if (order.Status == "Đang giao hàng")
            {
                // Nếu là COD thì cập nhật payment state để đảm bảo logic tích điểm (OrderService sẽ tích điểm khi paymentStatus == "Đã thanh toán")
                if (string.Equals(order.PaymentMethod, "cod", StringComparison.OrdinalIgnoreCase))
                {
                    await _orderService.UpdatePaymentStateAsync(id, "Hoàn tất", "Đã thanh toán", DateTime.Now);
                }
                else
                {
                    // Với các phương thức khác (prepaid) chỉ cập nhật status
                    await _orderService.UpdateStatusAsync(id, "Hoàn tất");
                }

                TempData["SuccessMessage"] = "Đã xác nhận hoàn tất đơn hàng #" + id + ". Bạn có thể đánh giá sản phẩm.";
            }

           return RedirectToPage("/Orders/Index", new { FilterStatus = this.FilterStatus, PageIndex = this.PageIndex});
        }
    }
}
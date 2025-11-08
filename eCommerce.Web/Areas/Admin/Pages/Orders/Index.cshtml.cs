// Đặt file này tại: Areas/Admin/Pages/Orders/Index.cshtml.cs

using eCommerce.Core.Entities; // Giả sử bạn có entity Order
using eCommerce.Core.Interfaces; // Giả sử bạn có IOrderRepository
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

// Namespace phải là .Areas.Admin.Pages.Orders
namespace eCommerce.Web.Areas.Admin.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly IOrderRepository _orderRepo;

        // Tiêm OrderRepository vào
        public IndexModel(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        // Thùng chứa danh sách đơn hàng
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();

        // Hàm OnGet để lấy dữ liệu
        public async Task OnGetAsync()
        {
            // Lấy tất cả đơn hàng (có thể kèm thông tin khách hàng)
            Orders = await _orderRepo.GetAllForAdminAsync();
            // Bạn cần tự định nghĩa hàm GetAllOrdersWithCustomerAsync() trong Repository
        }
    }
}
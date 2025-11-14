using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eCommerce.Web.Pages.Orders
{
    [Authorize]
    public class ChangeAddressModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;

        [BindProperty]
        public int OrderId { get; set; }

        // Tên thuộc tính PHẢI KHỚP với asp-for
        [BindProperty]
        public string ShippingName { get; set; }

        [BindProperty]
        public string ShippingAddress { get; set; }

        [BindProperty]
        public string Country { get; set; } // Đã đổi tên từ ShippingCountry

        [BindProperty]
        public string Province { get; set; } // Đã đổi tên từ ShippingCity

        public ChangeAddressModel(IOrderService orderService, AppDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Không thể thay đổi địa chỉ cho đơn hàng ở trạng thái này.";
                return RedirectToPage("/Orders/Index");
            }

            // Đổ dữ liệu cũ vào form
            OrderId = order.Id;
            ShippingName = order.ShippingName;
            ShippingAddress = order.ShippingAddress;
            Province = order.ShippingProvince; // Gán Province = ShippingCity từ DB
            Country = order.ShippingCountry; // Gán Country = ShippingCountry từ DB

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); 
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders.FindAsync(OrderId); 

            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được xử lý, không thể thay đổi địa chỉ.";
                return RedirectToPage("/Orders/Index");
            }

            // Cập nhật đơn hàng với dữ liệu MỚI từ form
            order.ShippingName = ShippingName;
            order.ShippingAddress = ShippingAddress;
            order.ShippingProvince = Province; // Cập nhật ShippingCity = Province từ form
            order.ShippingCountry = Country; // Cập nhật ShippingCountry = Country từ form

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật địa chỉ cho đơn hàng #{order.Id}.";
            return RedirectToPage("/Orders/Index");
        }
    }
}
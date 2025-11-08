using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages.Orders
{
    public class DetailsModel : PageModel
    {
        private readonly IOrderRepository _orderRepo;

        public DetailsModel(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        // Thùng chứa cho 1 đơn hàng
        public Order Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Dùng hàm chi tiết mà chúng ta đã tạo
            Order = await _orderRepo.GetDetailsByIdForAdminAsync(id);

            if (Order == null)
            {
                // Nếu không tìm thấy đơn hàng, quay về danh sách
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}
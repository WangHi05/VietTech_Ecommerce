using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces; // Cần dùng IOrderRepository
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eCommerce.Web.Areas.Admin.Pages.Customers
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepo; // Thêm repo đơn hàng

        public DetailsModel(UserManager<ApplicationUser> userManager, IOrderRepository orderRepo)
        {
            _userManager = userManager;
            _orderRepo = orderRepo;
        }

        public ApplicationUser Customer { get; set; }
        public IEnumerable<Order> Orders { get; set; } = new List<Order>();

        // ID của User là string (Guid), không phải int
        public async Task<IActionResult> OnGetAsync(string id) 
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Lấy thông tin khách hàng
            Customer = await _userManager.FindByIdAsync(id);

            if (Customer == null)
            {
                TempData["error"] = "Không tìm thấy khách hàng.";
                return RedirectToPage("./Index");
            }

            // Lấy lịch sử đơn hàng của khách hàng này
            Orders = await _orderRepo.GetByUserAsync(id); 

            return Page();
        }
    }
}
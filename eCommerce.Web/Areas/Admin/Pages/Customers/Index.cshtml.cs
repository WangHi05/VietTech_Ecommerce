using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic;
using System.Threading.Tasks;
using eCommerce.Core.Entities; 


namespace eCommerce.Web.Areas.Admin.Pages.Customers
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Thùng chứa danh sách khách hàng
        public IEnumerable<ApplicationUser> Customers { get; set; } = new List<ApplicationUser>();

        public async Task OnGetAsync()
        {
            // Lấy tất cả người dùng có Role "Customer"
            // "Customer" là tên Role bạn đã định nghĩa khi đăng ký
            Customers = await _userManager.GetUsersInRoleAsync("Customer");
            
            // Nếu bạn không dùng Role, bạn có thể lấy tất cả user
            // Customers = await _userManager.Users.ToListAsync();
        }
    }
}
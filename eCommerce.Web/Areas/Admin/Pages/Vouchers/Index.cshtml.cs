using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Areas.Admin.Pages.Vouchers
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Voucher> Vouchers { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Lấy danh sách voucher, sắp xếp cái mới tạo lên đầu
            Vouchers = await _context.Vouchers
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        // Chức năng Xóa Voucher
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);

            if (voucher != null)
            {
                // Kiểm tra nếu voucher đã có người dùng rồi thì không cho xóa (hoặc chỉ ẩn đi)
                var isUsed = await _context.UserVouchers.AnyAsync(uv => uv.VoucherId == id);
                if (isUsed)
                {
                    TempData["error"] = "Không thể xóa voucher này vì đã có khách hàng lưu hoặc sử dụng. Hãy tắt kích hoạt thay vì xóa.";
                    return RedirectToPage();
                }

                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã xóa voucher thành công.";
            }

            return RedirectToPage();
        }
    }
}
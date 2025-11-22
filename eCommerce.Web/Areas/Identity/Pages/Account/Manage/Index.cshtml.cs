#nullable disable

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
using eCommerce.Infrastructure.Data;
using eCommerce.Web.Services.Notifications;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;
        private readonly INotificationQueue _queue;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context,
            INotificationQueue queue)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _queue = queue;
        }

        public string Username { get; set; }
        public string ProfilePictureUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Họ và Tên")]
            [MaxLength(100)]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Vùng/Tỉnh")]
            [MaxLength(100)]
            public string Vung { get; set; }

            [Display(Name = "Ảnh đại diện mới")]
            public IFormFile Upload { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            ProfilePictureUrl = user.ProfilePictureUrl;

            Input = new InputModel
            {
                FullName = user.FullName,
                PhoneNumber = phoneNumber
                ,
                Vung = user.Vung
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            bool isProfileUpdated = false;

            // --- XỬ LÝ UPLOAD ẢNH RIÊNG BIỆT ---
            if (Input.Upload != null && Input.Upload.Length > 0)
            {
                // (Thêm kiểm tra loại file và kích thước ở đây nếu muốn)
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Input.Upload.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.Upload.CopyToAsync(fileStream);
                    }
                    user.ProfilePictureUrl = "/images/avatars/" + uniqueFileName;
                    // === SỬA LỖI: Ghi nhận thay đổi NGAY KHI chuẩn bị xong ảnh ===
                    isProfileUpdated = true;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi tải ảnh lên: " + ex.Message);
                    await LoadAsync(user);
                    return Page();
                }
            }

            // --- XỬ LÝ CẬP NHẬT THÔNG TIN KHÁC ---
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
                isProfileUpdated = true; // Ghi nhận có thay đổi
            }

            var previousVung = user.Vung ?? string.Empty;
            if ((Input.Vung ?? string.Empty) != previousVung)
            {
                user.Vung = Input.Vung;
                isProfileUpdated = true;
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach(var error in setPhoneResult.Errors) {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                } else {
                    isProfileUpdated = true; // Ghi nhận có thay đổi nếu thành công
                }
            }

            // --- LƯU THAY ĐỔI VÀO DB NẾU CÓ THAY ĐỔI VÀ KHÔNG CÓ LỖI ---
            if (isProfileUpdated && ModelState.IsValid)
            {
                 var updateResult = await _userManager.UpdateAsync(user);
                 if (!updateResult.Succeeded)
                 {
                       foreach(var error in updateResult.Errors) {
                           ModelState.AddModelError(string.Empty, error.Description);
                       }
                 } else {
                      StatusMessage = "Hồ sơ của bạn đã được cập nhật";

                      // If user's Vung changed, enqueue push notifications for recent vouchers matching new region
                      try
                      {
                          var newVung = (user.Vung ?? string.Empty).Trim();
                          if (!string.IsNullOrEmpty(newVung) && !string.Equals(newVung, previousVung, StringComparison.OrdinalIgnoreCase))
                          {
                              var now = DateTime.UtcNow;
                              var recentVouchers = await _context.Vouchers
                                  .Where(v => v.IsActive && v.ExpiryDate > now)
                                  .ToListAsync();

                              // Only notify vouchers that explicitly target the user's new region.
                              // Do NOT notify vouchers with empty Vung or "Toàn quốc" here.
                              var matching = recentVouchers.Where(v => !string.IsNullOrEmpty(v.Vung)
                                  && !v.Vung.Trim().Equals("Toàn quốc", StringComparison.OrdinalIgnoreCase)
                                  && v.Vung.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Contains(newVung, StringComparer.OrdinalIgnoreCase))
                                  .ToList();

                              foreach (var v in matching)
                              {
                                  var discountText = v.DiscountPercent.HasValue ? $"{v.DiscountPercent}%" : $"{v.DiscountAmount:N0}đ";
                                  var msg = new NotificationMessage
                                  {
                                      UserId = user.Id,
                                      Title = $"Có voucher phù hợp với vùng của bạn: {v.Code}",
                                      Body = $"{v.Description}. Giảm {discountText}. HSD: {v.ExpiryDate:dd/MM}.",
                                      Url = "/Vouchers",
                                      EnqueuedAt = DateTime.UtcNow
                                  };
                                  _queue.Enqueue(msg);
                              }
                          }
                      }
                      catch
                      {
                          // swallow errors from notification sending
                      }
                 }
            }
            else if (!isProfileUpdated && ModelState.IsValid)
            {
                 StatusMessage = "Không có thay đổi nào được thực hiện.";
            }

            // --- XỬ LÝ KẾT QUẢ ---
            if (!ModelState.IsValid)
            {
                 await LoadAsync(user);
                 return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }
    }
}

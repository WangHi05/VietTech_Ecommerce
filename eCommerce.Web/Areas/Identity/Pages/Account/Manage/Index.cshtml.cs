#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using eCommerce.Core.Entities;
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

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
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

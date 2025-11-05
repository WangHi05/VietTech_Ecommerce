#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using eCommerce.Core.Entities; // Using này đã chính xác
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace eCommerce.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        
        // === BƯỚC 1: THÊM RoleManager ===
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager) // === BƯỚC 2: Thêm tham số roleManager ===
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager; // === BƯỚC 3: Gán roleManager ===
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            // === BƯỚC 4: THÊM TRƯỜNG "Họ và Tên" ===
            [Required(ErrorMessage = "Họ và Tên là bắt buộc")]
            [Display(Name = "Họ và Tên")]
            [StringLength(100, ErrorMessage = "Họ tên phải dài từ 5 đến 100 ký tự.", MinimumLength = 5)]
            public string FullName { get; set; }
            // === HẾT BƯỚC 4 ===

            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Gán Email và UserName
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _userManager.SetEmailAsync(user, Input.Email); // Dùng UserManager để set email
                
                // === BƯỚC 5: GÁN HỌ VÀ TÊN (FullName) TỪ FORM ===
                user.FullName = Input.FullName;
                // === HẾT BƯỚC 5 ===
                
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Người dùng đã tạo tài khoản mới thành công.");

                    // === BƯỚC 6: TỰ ĐỘNG GÁN ROLE "Customer" ===
                    // Đảm bảo role "Customer" tồn tại (giống trong DbInitializer)
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }
                    // Gán role "Customer" cho user mới
                    await _userManager.AddToRoleAsync(user, "Customer");
                    // === HẾT BƯỚC 6 ===

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Xác nhận email của bạn",
                        $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Nếu có lỗi, hiển thị lại form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Không thể tạo một thực thể của '{nameof(ApplicationUser)}'. " +
                    $"Đảm bảo rằng '{nameof(ApplicationUser)}' không phải là một lớp abstract và có một hàm khởi tạo không tham số, hoặc " +
                    $"ghi đè trang đăng ký trong /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        // Không cần hàm này vì đã dùng _userManager.SetEmailAsync
        // private IUserEmailStore<ApplicationUser> GetEmailStore()
        // {
        //     if (!_userManager.SupportsUserEmail)
        //     {
        //         throw new NotSupportedException("The default UI requires a user store with email support.");
        //     }
        //     return (IUserEmailStore<ApplicationUser>)_userStore;
        // }
    }
}
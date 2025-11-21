using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Core.Entities
{
    // Kế thừa từ IdentityUser để có các thuộc tính mặc định như Email, UserName, PasswordHash...
    public class ApplicationUser : IdentityUser
    {
        [PersonalData] // Đánh dấu là dữ liệu cá nhân
        [MaxLength(100)]
        public string? FullName { get; set; }

        [PersonalData]
        public string? ProfilePictureUrl { get; set; }

        // Vùng / Tỉnh thành của người dùng (ví dụ: Toàn quốc, HCM, HaNoi)
        [PersonalData]
        [MaxLength(100)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Vùng/Tỉnh")]
        public string? Vung { get; set; }
    }
}

using eCommerce.Core.Entities;
using System.Threading.Tasks;

namespace eCommerce.Application.Strategies.Payment
{
    public interface IPaymentStrategy
    {
        // Tên của phương thức thanh toán (ví dụ: "COD", "VNPAY")
        // Dùng để phân biệt và chọn đúng Strategy khi khách hàng click đặt hàng
        string ProviderName { get; } 

        // Hàm xử lý thanh toán chính. 
        // Trả về một chuỗi string là URL để chuyển hướng người dùng 
        // (VNPAY thì chuyển sang trang của VNPAY, COD thì chuyển sang trang Success).
        Task<string> ExecutePaymentAsync(Order order);
    }
}
using eCommerce.Core.Entities;
using System.Threading.Tasks;

namespace eCommerce.Application.Strategies.Payment
{
    public class CodPaymentStrategy : IPaymentStrategy
    {
        public string ProviderName => "cod";

        public Task<string> ExecutePaymentAsync(Order order)
        {
            // Đối với COD, không cần gọi API cổng thanh toán.
            // Chỉ cần chuyển hướng thẳng tới trang thông báo thành công.
            string redirectUrl = $"/Payment/Result?orderId={order.Id}&success=True&method=COD";
    
            return Task.FromResult(redirectUrl);
        }
    }
}
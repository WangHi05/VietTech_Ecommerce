using eCommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Pages.Payment
{
    public class VnPayReturnModel : PageModel
    {
        private readonly IVnPayService _vnPayService;

        public VnPayReturnModel(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public bool PaymentSuccess { get; set; }
        public string PaymentMessage { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }

        public void OnGet()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.Success)
            {
                PaymentSuccess = true;
                PaymentMessage = "Thanh toán thành công!";
                OrderId = response.OrderId;
                TransactionId = response.TransactionId;
                PaymentMethod = response.PaymentMethod;
                OrderDescription = response.OrderDescription;

                // TODO: Cập nhật trạng thái đơn hàng trong database
                // UpdateOrderStatus(response.OrderId, "Paid");
            }
            else
            {
                PaymentSuccess = false;
                PaymentMessage = GetPaymentMessage(response.VnPayResponseCode);
            }
        }

        private string GetPaymentMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP). Xin quý khách vui lòng thực hiện lại giao dịch.",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định. Xin quý khách vui lòng thực hiện lại giao dịch",
                _ => "Giao dịch thất bại"
            };
        }
    }
}
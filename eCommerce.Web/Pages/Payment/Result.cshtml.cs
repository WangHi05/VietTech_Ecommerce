using eCommerce.Web.Services;
using eCommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eCommerce.Web.Pages.Payment
{
    public class ResultModel : PageModel
    {
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<ResultModel> _logger;
        private readonly IOrderService _orderService;

        public ResultModel(IVnPayService vnPayService, ILogger<ResultModel> logger, IOrderService orderService)
        {
            _vnPayService = vnPayService;
            _logger = logger;
            _orderService = orderService;
        }

        public bool PaymentSuccess { get; set; }
        public string PaymentMessage { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderDescription { get; set; } = string.Empty;

        // Nhận các param từ redirect hoặc VNPay callback
        public async Task OnGet(string orderId = "", bool success = false, string? method = null)
        {
            try
            {
                PaymentMethod = method?.ToLower() ?? "";

                if (Request.Query.ContainsKey("vnp_ResponseCode"))
                {
                    // VNPay callback
                    _logger.LogInformation("VNPay callback received");

                    foreach (var query in Request.Query)
                        _logger.LogInformation($"Query: {query.Key} = {query.Value}");

                    var response = _vnPayService.PaymentExecute(Request.Query);

                    PaymentSuccess = response.Success;
                    PaymentMessage = response.Success
                        ? "Thanh toán thành công!"
                        : GetPaymentMessage(response.VnPayResponseCode);

                    OrderId = response.OrderId;
                    TransactionId = response.TransactionId;
                    PaymentMethod = "vnpay";
                    OrderDescription = response.OrderDescription;

                    _logger.LogInformation($"VNPay payment result for order {OrderId}: {PaymentSuccess}");

                    if (PaymentSuccess)
                    {
                        try
                        {
                            if (int.TryParse(OrderId, out var oid))
                            {
                                // Cập nhật trạng thái thanh toán - OrderService sẽ phụ trách tích điểm khi paymentStatus == "Đã thanh toán"
                                await _orderService.UpdatePaymentStateAsync(oid, "Hoàn tất", "Đã thanh toán", DateTime.Now);
                                _logger.LogInformation($"Đã gọi UpdatePaymentStateAsync cho đơn hàng {OrderId}.");
                            }
                            else
                            {
                                _logger.LogWarning($"OrderId không phải số nguyên: {OrderId}. Bỏ qua cập nhật trạng thái/tích điểm.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái/tích điểm cho đơn hàng {OrderId}.");
                        }
                    }
                }

                else if (PaymentMethod == "card" || PaymentMethod == "cod")
                {
                    // Thanh toán thẻ OTP hoặc COD
                    PaymentSuccess = success;
                    PaymentMessage = success ? "Thanh toán thành công!" : "Thanh toán thất bại!";
                    OrderId = orderId;
                    TransactionId = ""; // Không có transaction id
                    OrderDescription = "";
                }
                else
                {
                    // Không nhận được method hợp lệ
                    PaymentSuccess = false;
                    PaymentMessage = "Không nhận được thông tin thanh toán hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                PaymentSuccess = false;
                PaymentMessage = $"Lỗi xử lý thanh toán: {ex.Message}";
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
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
                "75" => "Ngân hàng thanh toán đang bảo trì.",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
                _ => $"Giao dịch thất bại (Mã lỗi: {responseCode})"
            };
        }
    }
}

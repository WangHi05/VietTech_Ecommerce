using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json;

namespace eCommerce.Web.Controllers
{
    [Route("api/chatbot")] 
    [ApiController]
    
    public class ChatbotWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatbotWebhookController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("webhook")]
        
        public async Task<IActionResult> HandleWebhook([FromBody] DialogflowRequest request)
        {
            string responseText = "Xin lỗi, tôi chưa hiểu ý bạn.";
            string intentName = request.QueryResult?.Intent?.DisplayName;

            // Lấy tên sản phẩm (ví dụ: "laptop" hoặc "laptop dell")
            string productName = request.QueryResult.Parameters.ProductName?.ToString();


            // Lấy orderIdStr một lần ở đây vì cả hai Intent đều dùng
            string orderIdStr = request.QueryResult.Parameters.OrderId?.ToString();

            // Cố gắng parse orderId
            double.TryParse(orderIdStr, 
                            System.Globalization.NumberStyles.Any, 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            out double orderIdDouble);
            
            int orderId = (int)orderIdDouble;

            // === XỬ LÝ INTENT: KIỂM TRA TRẠNG THÁI ===
            if (intentName == "KiemTraDonHang")
            {
                if (orderId > 0)
                {
                    var order = await _context.Orders
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order != null)
                    {
                        responseText = $"Chào bạn, đơn hàng #{order.Id} của bạn đang ở trạng thái: {order.Status}.";
                    }
                    else
                    {
                        responseText = $"Xin lỗi, tôi không tìm thấy đơn hàng nào có mã #{orderIdStr}.";
                    }
                }
                else
                {
                    responseText = "Vui lòng cho tôi biết mã đơn hàng (ví dụ: #123) để tôi kiểm tra nhé!";
                }
            }
            
           else if (intentName == "ChiTietDonHang")
            {
                if (orderId > 0)
                {
                    // Truy vấn đơn hàng VÀ các sản phẩm liên quan (dùng .Include())
                    var order = await _context.Orders
                                        .AsNoTracking()
                                        .Include(o => o.Items) // <-- Vẫn cần kiểm tra tên này trong class Order
                                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order != null)
                    {
                        // Kiểm tra xem có thực sự tải được OrderItems không
                        if (order.Items != null && order.Items.Any())
                        {
                            responseText = $"Đơn hàng #{order.Id} của bạn bao gồm:\n";
                            
                            foreach (var item in order.Items)
                            {
                                responseText += $"- {item.Name} (SL: {item.Quantity}) - Giá: {item.Price:N0} VNĐ\n";
                            }
                            
                           
                            responseText += $"Tổng cộng: {order.Total:N0} VNĐ";
                        }
                        else
                        {
                            responseText = $"Đơn hàng #{order.Id} được tìm thấy nhưng không có sản phẩm nào (hoặc lỗi tải OrderItems).";
                        }
                    }
                    else
                    {
                        responseText = $"Xin lỗi, tôi không tìm thấy đơn hàng nào có mã #{orderIdStr}.";
                    }
                }
                else
                {
                    responseText = "Vui lòng cho tôi biết mã đơn hàng để tôi xem chi tiết nhé!";
                }
            }
            else if (intentName == "TraCuuSanPham")
            {
                
                if (!string.IsNullOrEmpty(productName))
                {
                    // 1. Tìm TẤT CẢ sản phẩm có chứa tên
                    var products = await _context.Products
                                        .AsNoTracking()
                                        .Where(p => p.Name.ToLower().Contains(productName.ToLower()))
                                        .ToListAsync();

                    // 2. Xử lý dựa trên số lượng tìm được
                    if (products.Count == 0)
                    {
                        // Nếu không tìm thấy
                        responseText = $"Xin lỗi, tôi không tìm thấy sản phẩm nào có tên '{productName}'.";
                    }
                    else if (products.Count == 1)
                    {
                        // Nếu tìm thấy 1 sản phẩm (hỏi cụ thể)
                        var product = products.First(); // Lấy sản phẩm duy nhất
                        responseText = $"Sản phẩm '{product.Name}':\n" +
                                    $"- Giá: {product.Price:N0} VNĐ\n" +
                                    $"- Tình trạng: Còn {product.StockQuantity} sản phẩm.\n" + // Đổi tên cột nếu cần
                                    $"- Mô tả: {product.Description}"; // Đổi tên cột nếu cần
                    }
                    else
                    {
                        // Nếu tìm thấy NHIỀU sản phẩm (hỏi chung, ví dụ: "laptop")
                        responseText = $"Tôi tìm thấy {products.Count} sản phẩm liên quan đến '{productName}'.\n" +
                                    "Dưới đây là một vài sản phẩm nổi bật:\n";
                        
                        // Lấy 3 sản phẩm đầu tiên để giới thiệu
                        foreach (var product in products.Take(3)) 
                        {
                            responseText += $"- {product.Name} (Giá: {product.Price:N0} VNĐ)\n";
                        }

                        if (products.Count > 3)
                        {
                            responseText += "Bạn có muốn xem thêm sản phẩm nào cụ thể không?";
                        }
                    }
                }
                else
                {
                    // Nếu Dialogflow không bắt được tên sản phẩm
                    responseText = "Bạn vui lòng cho tôi biết tên sản phẩm bạn muốn tra cứu nhé!";
                }
            }

            // Câu trả lời cuối cùng
            var response = new { fulfillmentText = responseText };
            return Ok(response);
        }
    }

    public class DialogflowRequest
    {
        [JsonPropertyName("queryResult")]
        public QueryResult? QueryResult { get; set; }
    }

    public class QueryResult
    {
        [JsonPropertyName("intent")]
        public Intent? Intent { get; set; }

        [JsonPropertyName("parameters")]
        public Parameters? Parameters { get; set; }
    }

    public class Intent
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }

    public class Parameters
    {
        [JsonPropertyName("order-id")]
        public object? OrderId { get; set; }

        [JsonPropertyName("product-name")]
        public string? ProductName { get; set; }
    }
}
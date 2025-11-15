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

            if (request.QueryResult?.Intent?.DisplayName == "KiemTraDonHang")
            {
                int orderId = request.QueryResult.Parameters.OrderId;

                // Nếu là số int, giá trị mặc định khi không có là 0
                if (orderId > 0)
                {
                    // Nếu lấy được số (orderId) thành công
                    var order = await _context.Orders
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order != null)
                    {
                        responseText = $"Chào bạn, đơn hàng #{order.Id} của bạn đang ở trạng thái: {order.Status}.";
                    }
                    else
                    {
                        responseText = $"Xin lỗi, tôi không tìm thấy đơn hàng nào có mã #{orderId}.";
                    }
                }
                else
                {
                    // Lỗi này xảy ra nếu Dialogflow không gửi 'order-id'
                    responseText = "Vui lòng cho tôi biết mã đơn hàng (ví dụ: #123) để tôi kiểm tra nhé!";
                }
            }

            var response = new { fulfillmentText = responseText };
            return Ok(response);
        }
    }

    // === CÁC LỚP MODEL ===
    public class DialogflowRequest
    {
        [JsonPropertyName("queryResult")]
        public QueryResult QueryResult { get; set; }
    }

    public class QueryResult
    {
        [JsonPropertyName("intent")]
        public Intent Intent { get; set; }

        [JsonPropertyName("parameters")]
        public Parameters Parameters { get; set; }
    }

    public class Intent
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }


    public class Parameters
    {
        [JsonPropertyName("order-id")]
        public int OrderId { get; set; }
    }
}
using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet("messages/{productId}")]
        public async Task<IActionResult> GetMessages(int productId)
        {
            var messages = await _context.Messages
                .Where(m => m.ProductId == productId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    m.Id,
                    m.ProductId,
                    m.SenderId,
                    m.SenderName,
                    m.Content,
                    m.ImageUrl,
                    m.CreatedAt,
                    m.IsFromSeller
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image provided");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/chat/{uniqueFileName}";
            return Ok(new { url = imageUrl });
        }
    }
}

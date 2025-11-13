using eCommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using eCommerce.Core.Entities;

namespace eCommerce.Web.Controllers
{
    [ApiController]
    [Route("api/push")]
    public class PushController : ControllerBase
    {
        private readonly AppDbContext _db;
        public PushController(AppDbContext db)
        {
            _db = db;
        }

    public class SubscribeRequest
    {
            public string? endpoint { get; set; }
            public Keys? keys { get; set; }
        }

        public class Keys
        {
            public string? p256dh { get; set; }
            public string? auth { get; set; }
        }

        [HttpPost("subscribe")]
        [Authorize]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (req == null || string.IsNullOrEmpty(req.endpoint) || req.keys == null) return BadRequest();

            var exists = await _db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == req.endpoint && s.UserId == userId);
            if (exists == null)
            {
                var s = new PushSubscription
                {
                    UserId = userId,
                    Endpoint = req.endpoint,
                    P256DH = req.keys.p256dh ?? string.Empty,
                    Auth = req.keys.auth ?? string.Empty
                };
                _db.PushSubscriptions.Add(s);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }
    }
}

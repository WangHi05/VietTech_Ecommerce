using Microsoft.AspNetCore.SignalR;
using eCommerce.Infrastructure.Data;
using eCommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace eCommerce.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(int productId, string content, string? imageUrl)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            // Check if user is admin/seller
            var isFromSeller = Context.User?.IsInRole("Admin") ?? false;

            var message = new Message
            {
                ProductId = productId,
                SenderId = userId,
                SenderName = userName,
                Content = content,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                IsFromSeller = isFromSeller
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Broadcast to all clients viewing this product
            await Clients.Group($"product_{productId}").SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                productId = message.ProductId,
                senderId = message.SenderId,
                senderName = message.SenderName,
                content = message.Content,
                imageUrl = message.ImageUrl,
                createdAt = message.CreatedAt,
                isFromSeller = message.IsFromSeller
            });
        }

        public async Task JoinProductChat(int productId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"product_{productId}");
        }

        public async Task LeaveProductChat(int productId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"product_{productId}");
        }
    }
}

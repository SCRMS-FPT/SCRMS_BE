using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Text.Json;
using Chat.API.Data.Repositories;

namespace Chat.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly StackExchange.Redis.IDatabase _redis;
        private readonly IChatSessionRepository _chatSessionRepository;

        public ChatHub(IConnectionMultiplexer redis, IChatSessionRepository chatSessionRepository)
        {
            _redis = redis.GetDatabase();
            _chatSessionRepository = chatSessionRepository;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _redis.HashSetAsync("online_status", userId, "online");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _redis.HashSetAsync("online_status", userId, "offline");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(Guid sessionId, string message)
        {
            // Rate limiting
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rateLimitKey = $"rate_limit:{userId}";
            var currentCount = await _redis.StringIncrementAsync(rateLimitKey);
            if (currentCount == 1)
            {
                await _redis.KeyExpireAsync(rateLimitKey, TimeSpan.FromMinutes(1));
            }

            if (currentCount > 30)
            {
                throw new Exception("Rate limit exceeded");
            }

            // Lưu tin nhắn tạm
            var msg = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = sessionId,
                SenderId = Guid.Parse(userId),
                MessageText = message,
                SentAt = DateTime.UtcNow
            };

            await _redis.ListRightPushAsync($"chat:{sessionId}", JsonSerializer.Serialize(msg));
            await Clients.Group(sessionId.ToString()).SendAsync("ReceiveMessage", msg);
        }

        public async Task JoinChatSession(string sessionId)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Kiểm tra xem người dùng có quyền tham gia phiên chat này không
            // (cần inject IChatSessionRepository vào hub)
            var session = await _chatSessionRepository.GetChatSessionByIdAsync(Guid.Parse(sessionId));
            if (session == null)
            {
                throw new HubException("Chat session not found");
            }

            if (session.User1Id != Guid.Parse(userId) && session.User2Id != Guid.Parse(userId))
            {
                throw new HubException("You do not have permission to join this chat session");
            }

            // Tham gia vào group
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Chat.API.Hubs
{
    public class ChatHub : Hub
    {
        private readonly StackExchange.Redis.IDatabase _redis;

        public ChatHub(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
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
    }
}
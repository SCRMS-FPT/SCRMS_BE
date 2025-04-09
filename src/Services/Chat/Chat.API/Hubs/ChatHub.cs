using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
using StackExchange.Redis;
using System.Text.RegularExpressions;
using System.Text.Json;
using Chat.API.Data.Repositories;
using Microsoft.IdentityModel.JsonWebTokens;

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
            var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _redis.HashSetAsync("online_status", userId, "online");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)
                                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _redis.HashSetAsync("online_status", userId, "offline");
            }
            await base.OnDisconnectedAsync(exception);
        }

        [Obsolete("Use the REST API endpoint POST /api/chats/{chatSessionId}/messages instead")]
        public async Task SendMessage(Guid sessionId, string message)
        {
            // Chuyển hướng đến API endpoint (không xử lý ở đây)
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            throw new HubException("This method is deprecated. Please use the REST API endpoint POST /api/chats/{chatSessionId}/messages instead.");
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
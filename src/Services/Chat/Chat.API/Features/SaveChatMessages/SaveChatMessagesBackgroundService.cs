using Chat.API.Data;
using Chat.API.Data.Repositories;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Text.Json;

namespace Chat.API.Features.SaveChatMessages
{
    public class SaveChatMessagesBackgroundService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly ILogger<SaveChatMessagesBackgroundService> _logger;

        public SaveChatMessagesBackgroundService(
            IConnectionMultiplexer redis,
            IChatMessageRepository chatMessageRepository,
            ILogger<SaveChatMessagesBackgroundService> logger)
        {
            _redis = redis;
            _chatMessageRepository = chatMessageRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var db = _redis.GetDatabase();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Quét tất cả các keys chat:*
                    var server = _redis.GetServer(_redis.GetEndPoints().First());
                    var keys = server.Keys(pattern: "chat:*").ToArray();

                    foreach (var key in keys)
                    {
                        // Lấy tất cả tin nhắn từ Redis và lưu vào database
                        var messages = await db.ListRangeAsync(key);
                        if (messages.Length > 0)
                        {
                            foreach (var messageJson in messages)
                            {
                                var message = JsonSerializer.Deserialize<ChatMessage>(messageJson);
                                if (message != null)
                                {
                                    await _chatMessageRepository.AddChatMessageAsync(message);
                                }
                            }

                            // Xóa các tin nhắn đã xử lý khỏi Redis
                            await db.KeyDeleteAsync(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing chat messages from Redis");
                }

                // Chờ một khoảng thời gian trước khi quét lại
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
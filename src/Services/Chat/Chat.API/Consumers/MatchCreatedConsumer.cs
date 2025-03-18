using Chat.API.Data.Repositories;
using Chat.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Chat.API.Consumers;

public class MatchCreatedConsumer : IConsumer<MatchCreatedEvent>
{
    private readonly IChatSessionRepository _sessionRepo;
    private readonly IHubContext<ChatHub> _hubContext;

    public MatchCreatedConsumer(
        IChatSessionRepository sessionRepo,
        IHubContext<ChatHub> hubContext)
    {
        _sessionRepo = sessionRepo;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<MatchCreatedEvent> context)
    {
        var message = context.Message;

        var session = new ChatSession
        {
            Id = Guid.NewGuid(),
            User1Id = message.UserId1,
            User2Id = message.UserId2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _sessionRepo.AddChatSessionAsync(session);
        await _hubContext.Clients
            .Users(message.UserId1.ToString(), message.UserId2.ToString())
            .SendAsync("NewMatch", session.Id);
    }
}
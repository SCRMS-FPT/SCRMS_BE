using Chat.API.Data;
using Chat.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.SendMessage
{
    public record SendMessageCommand(Guid ChatSessionId, Guid SenderId, string MessageText) : IRequest<SendMessageResult>;

    public record SendMessageResult(Guid MessageId);

    public class SendMessageHandler : IRequestHandler<SendMessageCommand, SendMessageResult>
    {
        private readonly ChatDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public SendMessageHandler(ChatDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<SendMessageResult> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var chatSession = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => cs.Id == request.ChatSessionId, cancellationToken);

            if (chatSession == null)
                throw new Exception("Chat session not found");

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = request.ChatSessionId,
                SenderId = request.SenderId,
                MessageText = request.MessageText,
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            chatSession.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Thông báo qua SignalR
            var partnerId = chatSession.User1Id == request.SenderId ? chatSession.User2Id : chatSession.User1Id;
            await _hubContext.Clients.User(partnerId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.ChatSessionId,
                message.SenderId,
                message.MessageText,
                message.SentAt
            });

            return new SendMessageResult(message.Id);
        }
    }
}
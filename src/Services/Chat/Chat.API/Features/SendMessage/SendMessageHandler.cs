using Chat.API.Data;
using Chat.API.Data.Repositories;
using Chat.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.SendMessage
{
    public record SendMessageCommand(Guid ChatSessionId, Guid SenderId, string MessageText) : IRequest<ChatMessage>;

    public class SendMessageHandler : IRequestHandler<SendMessageCommand, ChatMessage>
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public SendMessageHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<ChatMessage> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            if (request.ChatSessionId == Guid.Empty)
                throw new Exception("ChatSessionId cannot be empty");

            if (string.IsNullOrWhiteSpace(request.MessageText))
                throw new Exception("Message text cannot be empty");

            var chatMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = request.ChatSessionId,
                SenderId = request.SenderId,
                MessageText = request.MessageText,
                SentAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatMessageRepository.AddChatMessageAsync(chatMessage);
            return chatMessage;
        }
    }
}
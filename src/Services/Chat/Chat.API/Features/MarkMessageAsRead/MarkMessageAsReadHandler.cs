using Chat.API.Data;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.MarkMessageAsRead
{
    public record MarkMessageAsReadCommand(Guid ChatSessionId, Guid MessageId, Guid UserId) : IRequest;

    public class MarkMessageAsReadHandler : IRequestHandler<MarkMessageAsReadCommand>
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public MarkMessageAsReadHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
        {
            if (request.MessageId == Guid.Empty)
                throw new Exception("MessageId cannot be empty");

            var message = await _chatMessageRepository.GetChatMessageByIdAsync(request.MessageId);
            if (message == null)
                throw new Exception("Message not found");

            message.ReadAt = DateTime.UtcNow;
            await _chatMessageRepository.UpdateChatMessageAsync(message);
        }
    }
}
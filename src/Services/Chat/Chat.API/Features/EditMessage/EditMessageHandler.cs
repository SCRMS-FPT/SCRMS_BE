using Chat.API.Data;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.EditMessage
{
    public record EditMessageCommand(Guid ChatSessionId, Guid MessageId, string MessageText, Guid UserId) : IRequest;

    public class EditMessageHandler : IRequestHandler<EditMessageCommand>
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public EditMessageHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task Handle(EditMessageCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra nếu MessageText rỗng hoặc chỉ chứa khoảng trắng
            if (string.IsNullOrWhiteSpace(request.MessageText))
                throw new Exception("Message text cannot be empty");

            var message = await _chatMessageRepository.GetChatMessageByIdAndSessionAsync(request.MessageId, request.ChatSessionId, request.UserId);
            if (message == null)
                throw new Exception("Message not found or not authorized");

            message.MessageText = request.MessageText;
            message.UpdatedAt = DateTime.UtcNow;
            await _chatMessageRepository.UpdateChatMessageAsync(message);
        }
    }
}
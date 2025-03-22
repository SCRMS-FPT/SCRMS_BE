using Chat.API.Data;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.GetChatMessages
{
    public record GetChatMessagesQuery(Guid ChatSessionId, int Page, int Limit) : IRequest<List<ChatMessage>>;

    public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesQuery, List<ChatMessage>>
    {
        private readonly IChatMessageRepository _chatMessageRepository;

        public GetChatMessagesHandler(IChatMessageRepository chatMessageRepository)
        {
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<List<ChatMessage>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            if (request.Page <= 0)
                throw new Exception("Page must be greater than 0");

            return await _chatMessageRepository.GetChatMessageByChatSessionIdAsync(request.ChatSessionId, request.Page, request.Limit);
        }
    }
}
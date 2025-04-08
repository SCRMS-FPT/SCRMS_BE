using Chat.API.Data;
using Chat.API.Data.Repositories;

namespace Chat.API.Features.GetChatSessionByUsers
{
    public record GetChatSessionByUsersQuery(Guid User1Id, Guid User2Id) : IRequest<ChatSessionDetailResponse?>;

    public record ChatSessionDetailResponse(
        Guid ChatSessionId,
        Guid User1Id,
        Guid User2Id,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    public class GetChatSessionByUsersHandler : IRequestHandler<GetChatSessionByUsersQuery, ChatSessionDetailResponse?>
    {
        private readonly IChatSessionRepository _chatSessionRepository;

        public GetChatSessionByUsersHandler(IChatSessionRepository chatSessionRepository)
        {
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<ChatSessionDetailResponse?> Handle(GetChatSessionByUsersQuery request, CancellationToken cancellationToken)
        {
            var chatSession = await _chatSessionRepository.GetChatSessionByUsersAsync(request.User1Id, request.User2Id);

            if (chatSession == null)
                return null;

            return new ChatSessionDetailResponse(
                ChatSessionId: chatSession.Id,
                User1Id: chatSession.User1Id,
                User2Id: chatSession.User2Id,
                CreatedAt: chatSession.CreatedAt,
                UpdatedAt: chatSession.UpdatedAt
            );
        }
    }
}
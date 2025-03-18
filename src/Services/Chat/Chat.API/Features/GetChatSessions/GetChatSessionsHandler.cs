using Chat.API.Data;
using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.GetChatSessions
{
    public record GetChatSessionsQuery(int Page, int Limit, Guid UserId) : IRequest<List<ChatSessionResponse>>;

    public record ChatSessionResponse(Guid Id, Guid PartnerId, DateTime UpdatedAt);

    public class GetChatSessionsHandler : IRequestHandler<GetChatSessionsQuery, List<ChatSessionResponse>>
    {
        private readonly IChatSessionRepository _chatSessionRepository;

        public GetChatSessionsHandler(IChatSessionRepository chatSessionRepository)
        {
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<List<ChatSessionResponse>> Handle(GetChatSessionsQuery request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty)
                throw new Exception("UserId cannot be empty");

            return await _chatSessionRepository.GetChatSessionByUserIdAsync(request.UserId);
        }
    }
}
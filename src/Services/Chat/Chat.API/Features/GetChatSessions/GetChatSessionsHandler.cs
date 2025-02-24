using Chat.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.GetChatSessions
{
    public record GetChatSessionsQuery(int Page, int Limit, Guid UserId) : IRequest<List<ChatSessionResponse>>;

    public record ChatSessionResponse(Guid Id, Guid PartnerId, DateTime UpdatedAt);

    public class GetChatSessionsHandler : IRequestHandler<GetChatSessionsQuery, List<ChatSessionResponse>>
    {
        private readonly ChatDbContext _context;

        public GetChatSessionsHandler(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatSessionResponse>> Handle(GetChatSessionsQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var chatSessions = await _context.ChatSessions
                .Where(cs => cs.User1Id == userId || cs.User2Id == userId)
                .OrderByDescending(cs => cs.UpdatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(cs => new ChatSessionResponse(
                    cs.Id,
                    cs.User1Id == userId ? cs.User2Id : cs.User1Id,
                    cs.UpdatedAt
                ))
                .ToListAsync(cancellationToken);

            return chatSessions;
        }
    }
}
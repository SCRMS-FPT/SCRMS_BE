using Chat.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.GetChatMessages
{
    public record GetChatMessagesQuery(Guid ChatSessionId, int Page, int Limit) : IRequest<List<ChatMessageResponse>>;

    public record ChatMessageResponse(Guid Id, Guid SenderId, string MessageText, DateTime SentAt, DateTime? ReadAt);

    public class GetChatMessagesHandler : IRequestHandler<GetChatMessagesQuery, List<ChatMessageResponse>>
    {
        private readonly ChatDbContext _context;

        public GetChatMessagesHandler(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessageResponse>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
        {
            var messages = await _context.ChatMessages
                .Where(cm => cm.ChatSessionId == request.ChatSessionId)
                .OrderBy(cm => cm.SentAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(cm => new ChatMessageResponse(
                    cm.Id,
                    cm.SenderId,
                    cm.MessageText,
                    cm.SentAt,
                    cm.ReadAt
                ))
                .ToListAsync(cancellationToken);

            return messages;
        }
    }
}
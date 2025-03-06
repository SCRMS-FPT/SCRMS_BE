using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.CreateChatSession
{
    public record CreateChatSessionCommand(Guid User1Id, Guid User2Id) : IRequest<CreateChatSessionResult>;

    public record CreateChatSessionResult(Guid ChatSessionId);

    public class CreateChatSessionHandler : IRequestHandler<CreateChatSessionCommand, CreateChatSessionResult>
    {
        private readonly ChatDbContext _context;

        public CreateChatSessionHandler(ChatDbContext context)
        {
            _context = context;
        }

        public async Task<CreateChatSessionResult> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra phiên chat đã tồn tại chưa
            var existingSession = await _context.ChatSessions
                .FirstOrDefaultAsync(cs => (cs.User1Id == request.User1Id && cs.User2Id == request.User2Id) ||
                                           (cs.User1Id == request.User2Id && cs.User2Id == request.User1Id), cancellationToken);

            if (existingSession != null)
                return new CreateChatSessionResult(existingSession.Id);

            // Tạo phiên chat mới
            var chatSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = request.User1Id,
                User2Id = request.User2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChatSessions.Add(chatSession);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateChatSessionResult(chatSession.Id);
        }
    }
}
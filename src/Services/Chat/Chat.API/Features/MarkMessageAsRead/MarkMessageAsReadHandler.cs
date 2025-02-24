using Chat.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.MarkMessageAsRead
{
    public record MarkMessageAsReadCommand(Guid ChatSessionId, Guid MessageId, Guid UserId) : IRequest;

    public class MarkMessageAsReadHandler : IRequestHandler<MarkMessageAsReadCommand>
    {
        private readonly ChatDbContext _context;

        public MarkMessageAsReadHandler(ChatDbContext context)
        {
            _context = context;
        }

        public async Task Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId && cm.ChatSessionId == request.ChatSessionId && cm.SenderId != request.UserId, cancellationToken);

            if (message == null)
                throw new Exception("Message not found or not authorized");

            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
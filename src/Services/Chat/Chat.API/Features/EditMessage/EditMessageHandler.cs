using Chat.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.EditMessage
{
    public record EditMessageCommand(Guid ChatSessionId, Guid MessageId, string MessageText, Guid UserId) : IRequest;

    public class EditMessageHandler : IRequestHandler<EditMessageCommand>
    {
        private readonly ChatDbContext _context;

        public EditMessageHandler(ChatDbContext context)
        {
            _context = context;
        }

        public async Task Handle(EditMessageCommand request, CancellationToken cancellationToken)
        {
            var message = await _context.ChatMessages
                .FirstOrDefaultAsync(cm => cm.Id == request.MessageId && cm.ChatSessionId == request.ChatSessionId && cm.SenderId == request.UserId, cancellationToken);

            if (message == null)
                throw new Exception("Message not found or not authorized");

            message.MessageText = request.MessageText;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
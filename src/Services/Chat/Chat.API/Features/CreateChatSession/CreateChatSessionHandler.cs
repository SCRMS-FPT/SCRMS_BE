using Chat.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Chat.API.Features.CreateChatSession
{
    public record CreateChatSessionCommand(Guid User1Id, Guid User2Id) : IRequest<CreateChatSessionResult>;

    public record CreateChatSessionResult(Guid ChatSessionId);

    public class CreateChatSessionCommandValidator : AbstractValidator<CreateChatSessionCommand>
    {
        public CreateChatSessionCommandValidator()
        {
            RuleFor(x => x.User2Id)
                .NotEmpty().WithMessage("User2Id cannot be empty");
        }
    }

    public class CreateChatSessionHandler : IRequestHandler<CreateChatSessionCommand, CreateChatSessionResult>
    {
        private readonly IChatSessionRepository _chatSessionRepository;

        public CreateChatSessionHandler(IChatSessionRepository chatSessionRepository)
        {
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<CreateChatSessionResult> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
        {
            if (request.User1Id == request.User2Id)
                throw new Exception("User1Id and User2Id cannot be the same");

            var existingSession = await _chatSessionRepository.GetChatSessionByUsersAsync(request.User1Id, request.User2Id);
            if (existingSession != null)
                return new CreateChatSessionResult(existingSession.Id);

            var chatSession = new ChatSession
            {
                Id = Guid.NewGuid(),
                User1Id = request.User1Id,
                User2Id = request.User2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatSessionRepository.AddChatSessionAsync(chatSession);
            return new CreateChatSessionResult(chatSession.Id);
        }
    }
}
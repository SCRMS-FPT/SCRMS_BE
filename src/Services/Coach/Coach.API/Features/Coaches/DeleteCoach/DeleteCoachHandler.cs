using Coach.API.Data.Repositories;
using BuildingBlocks.Exceptions;

namespace Coach.API.Features.Coaches.DeleteCoach
{
    public class DeleteCoachHandler : ICommandHandler<DeleteCoachCommand, bool>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ILogger<DeleteCoachHandler> _logger;

        public DeleteCoachHandler(ICoachRepository coachRepository, ILogger<DeleteCoachHandler> logger)
        {
            _coachRepository = coachRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteCoachCommand request, CancellationToken cancellationToken)
        {
            var exists = await _coachRepository.CoachExistsAsync(request.CoachId, cancellationToken);

            if (!exists)
                throw new NotFoundException("Coach", request.CoachId);

            await _coachRepository.SetCoachStatusAsync(request.CoachId, "deleted", cancellationToken);

            _logger.LogInformation("Coach {CoachId} has been soft deleted", request.CoachId);

            return true;
        }
    }
}
using Matching.API.Data;
using Matching.API.Data.Repositories;
using Matching.API.Features.Swipe;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.RespondToSwipe
{
    public record RespondToSwipeCommand(Guid SwipeActionId, string Decision, Guid UserId) : IRequest<SwipeResult>;

    public class RespondToSwipeHandler : IRequestHandler<RespondToSwipeCommand, SwipeResult>
    {
        private readonly ISwipeActionRepository _swipeActionRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly MatchingDbContext _context;

        public RespondToSwipeHandler(
            ISwipeActionRepository swipeActionRepository,
            IMatchRepository matchRepository,
            MatchingDbContext context)
        {
            _swipeActionRepository = swipeActionRepository;
            _matchRepository = matchRepository;
            _context = context;
        }

        public async Task<SwipeResult> Handle(RespondToSwipeCommand request, CancellationToken cancellationToken)
        {
            var swipeAction = await _swipeActionRepository.GetByIdAsync(request.SwipeActionId, cancellationToken);
            if (swipeAction == null || swipeAction.SwipedUserId != request.UserId)
                throw new Exception("Swipe action not found or unauthorized");

            swipeAction.Decision = request.Decision;
            await _swipeActionRepository.UpdateSwipeActionAsync(swipeAction, cancellationToken);

            if (request.Decision == "accepted")
            {
                var reverseSwipe = await _swipeActionRepository.GetBySwiperAndSwipedAsync(request.UserId, swipeAction.SwiperId, cancellationToken);
                if (reverseSwipe != null)
                {
                    reverseSwipe.Decision = "accepted";
                    await _swipeActionRepository.UpdateSwipeActionAsync(reverseSwipe, cancellationToken);

                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        InitiatorId = swipeAction.SwiperId,
                        MatchedUserId = request.UserId,
                        MatchTime = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _matchRepository.AddMatchAsync(match, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new SwipeResult(true);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new SwipeResult(false);
        }
    }
}
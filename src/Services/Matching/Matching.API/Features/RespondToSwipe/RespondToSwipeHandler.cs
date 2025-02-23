using Matching.API.Data;
using Matching.API.Features.Swipe;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.RespondToSwipe
{
    public record RespondToSwipeCommand(Guid SwipeActionId, string Decision, Guid UserId) : IRequest<SwipeResult>;

    public class RespondToSwipeHandler : IRequestHandler<RespondToSwipeCommand, SwipeResult>
    {
        private readonly MatchingDbContext _context;

        public RespondToSwipeHandler(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<SwipeResult> Handle(RespondToSwipeCommand request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var swipeAction = await _context.SwipeActions
                .FirstOrDefaultAsync(sa => sa.Id == request.SwipeActionId && sa.SwipedUserId == userId, cancellationToken);

            if (swipeAction == null) throw new Exception("Swipe action not found");

            swipeAction.Decision = request.Decision;
            if (request.Decision == "accepted")
            {
                var reverseSwipe = await _context.SwipeActions
                    .FirstOrDefaultAsync(sa => sa.SwiperId == userId &&
                                              sa.SwipedUserId == swipeAction.SwiperId &&
                                              sa.Decision == "pending", cancellationToken);

                if (reverseSwipe != null)
                {
                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        InitiatorId = swipeAction.SwiperId,
                        MatchedUserId = userId,
                        MatchTime = DateTime.UtcNow,
                        Status = "confirmed",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Matches.Add(match);
                    await _context.SaveChangesAsync(cancellationToken);
                    return new SwipeResult(true);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new SwipeResult(false);
        }
    }
}
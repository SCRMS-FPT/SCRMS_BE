using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Swipe
{
    public record SwipeCommand(Guid SwipedUserId, string Decision, Guid SwiperId) : IRequest<SwipeResult>;

    public record SwipeResult(bool IsMatch);

    public class SwipeHandler : IRequestHandler<SwipeCommand, SwipeResult>
    {
        private readonly MatchingDbContext _context;

        public SwipeHandler(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<SwipeResult> Handle(SwipeCommand request, CancellationToken cancellationToken)
        {
            var swiperId = request.SwiperId;
            var swipeAction = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = swiperId,
                SwipedUserId = request.SwipedUserId,
                Decision = request.Decision,
                CreatedAt = DateTime.UtcNow
            };

            _context.SwipeActions.Add(swipeAction);

            if (request.Decision == "pending")
            {
                var reverseSwipe = await _context.SwipeActions
                    .FirstOrDefaultAsync(sa => sa.SwiperId == request.SwipedUserId &&
                                              sa.SwipedUserId == swiperId &&
                                              sa.Decision == "accepted", cancellationToken);

                if (reverseSwipe != null)
                {
                    var match = new Match
                    {
                        Id = Guid.NewGuid(),
                        InitiatorId = swiperId,
                        MatchedUserId = request.SwipedUserId,
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
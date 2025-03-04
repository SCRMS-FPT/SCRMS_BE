using MediatR;
using Microsoft.EntityFrameworkCore;
using Matching.API.Data;
using Matching.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Matching.API.Data.Repositories;

namespace Matching.API.Features.Swipe
{
    public record SwipeCommand(Guid SwipedUserId, string Decision, Guid SwiperId) : IRequest<SwipeResult>;

    public record SwipeResult(bool IsMatch);

    public class SwipeHandler : IRequestHandler<SwipeCommand, SwipeResult>
    {
        private readonly ISwipeActionRepository _swipeActionRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly MatchingDbContext _context;

        public SwipeHandler(
            ISwipeActionRepository swipeActionRepository,
            IMatchRepository matchRepository,
            MatchingDbContext context)
        {
            _swipeActionRepository = swipeActionRepository;
            _matchRepository = matchRepository;
            _context = context;
        }

        public async Task<SwipeResult> Handle(SwipeCommand request, CancellationToken cancellationToken)
        {
            var reverseSwipe = await _swipeActionRepository.GetBySwiperAndSwipedAsync(request.SwipedUserId, request.SwiperId, cancellationToken);
            string finalDecision = reverseSwipe == null ? "pending" : "accepted";
            if (request.Decision == "reject") finalDecision = "rejected";

            var swipeAction = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = request.SwiperId,
                SwipedUserId = request.SwipedUserId,
                Decision = finalDecision,
                CreatedAt = DateTime.UtcNow
            };
            await _swipeActionRepository.AddSwipeActionAsync(swipeAction, cancellationToken);

            bool isMatch = false;
            if (finalDecision == "accepted" && reverseSwipe != null)
            {
                reverseSwipe.Decision = "accepted";
                await _swipeActionRepository.UpdateSwipeActionAsync(reverseSwipe, cancellationToken);

                var match = new Match
                {
                    Id = Guid.NewGuid(),
                    InitiatorId = request.SwipedUserId,
                    MatchedUserId = request.SwiperId,
                    MatchTime = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _matchRepository.AddMatchAsync(match, cancellationToken);
                isMatch = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new SwipeResult(isMatch);
        }
    }
}
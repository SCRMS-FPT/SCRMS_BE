using MediatR;
using Microsoft.EntityFrameworkCore;
using Matching.API.Data;
using Matching.API.Data.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Matching.API.Features.Swipe
{
    public record SwipeCommand(Guid SwipedUserId, string Decision, Guid SwiperId) : IRequest<SwipeResult>;

    public record SwipeResult(bool IsMatch);

    public class SwipeHandler : IRequestHandler<SwipeCommand, SwipeResult>
    {
        private readonly MatchingDbContext _context;

        public SwipeHandler(MatchingDbContext context) => _context = context;

        public async Task<SwipeResult> Handle(SwipeCommand request, CancellationToken cancellationToken)
        {
            var swiperId = request.SwiperId;
            var swipedUserId = request.SwipedUserId;

            // Kiểm tra xem người kia đã swipe mình chưa
            var reverseSwipe = await _context.SwipeActions
                .FirstOrDefaultAsync(sa => sa.SwiperId == swipedUserId && sa.SwipedUserId == swiperId && sa.Decision == "pending", cancellationToken);

            // Xác định decision cuối cùng
            string finalDecision = reverseSwipe == null ? "pending" : "accepted";
            if (request.Decision == "reject") finalDecision = "rejected";

            // Tạo bản ghi swipe mới
            var swipeAction = new SwipeAction
            {
                Id = Guid.NewGuid(),
                SwiperId = swiperId,
                SwipedUserId = swipedUserId,
                Decision = finalDecision,
                CreatedAt = DateTime.UtcNow
            };
            _context.SwipeActions.Add(swipeAction);

            bool isMatch = false;
            if (finalDecision == "accepted" && reverseSwipe != null)
            {
                // Cập nhật bản ghi của người kia thành "accepted"
                reverseSwipe.Decision = "accepted";

                // Tạo bản ghi match
                var match = new Match
                {
                    Id = Guid.NewGuid(),
                    InitiatorId = swipedUserId, // Người swipe trước là initiator
                    MatchedUserId = swiperId,
                    MatchTime = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Matches.Add(match);
                isMatch = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return new SwipeResult(isMatch);
        }
    }
}
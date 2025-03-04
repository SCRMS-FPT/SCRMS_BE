using Microsoft.EntityFrameworkCore;
using Matching.API.Data.Models;

namespace Matching.API.Data.Repositories
{
    public class SwipeActionRepository : ISwipeActionRepository
    {
        private readonly MatchingDbContext _context;

        public SwipeActionRepository(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<SwipeAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.SwipeActions.FirstOrDefaultAsync(sa => sa.Id == id, cancellationToken);
        }

        public async Task<SwipeAction?> GetBySwiperAndSwipedAsync(Guid swiperId, Guid swipedUserId, CancellationToken cancellationToken)
        {
            return await _context.SwipeActions.FirstOrDefaultAsync(sa => sa.SwiperId == swiperId && sa.SwipedUserId == swipedUserId, cancellationToken);
        }

        public async Task<List<SwipeAction>> GetPendingSwipesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.SwipeActions
                .Where(sa => sa.SwipedUserId == userId && sa.Decision == "pending")
                .ToListAsync(cancellationToken);
        }

        public async Task AddSwipeActionAsync(SwipeAction swipeAction, CancellationToken cancellationToken)
        {
            await _context.SwipeActions.AddAsync(swipeAction, cancellationToken);
        }

        public Task UpdateSwipeActionAsync(SwipeAction swipeAction, CancellationToken cancellationToken)
        {
            _context.SwipeActions.Update(swipeAction);
            return Task.CompletedTask;
        }
    }
}
namespace Matching.API.Data.Repositories
{
    public interface ISwipeActionRepository
    {
        Task<SwipeAction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<SwipeAction?> GetBySwiperAndSwipedAsync(Guid swiperId, Guid swipedUserId, CancellationToken cancellationToken);

        Task<List<SwipeAction>> GetPendingSwipesByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task AddSwipeActionAsync(SwipeAction swipeAction, CancellationToken cancellationToken);

        Task UpdateSwipeActionAsync(SwipeAction swipeAction, CancellationToken cancellationToken);
    }
}
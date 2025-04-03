using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.PendingResponses.GetPendingSwipes
{
    public record GetPendingSwipesQuery(Guid UserId) : IRequest<List<PendingSwipeResponse>>;

    public record PendingSwipeResponse(Guid SwipeActionId, Guid SwiperId, DateTime CreatedAt);

    public class GetPendingSwipesHandler : IRequestHandler<GetPendingSwipesQuery, List<PendingSwipeResponse>>
    {
        private readonly ISwipeActionRepository _swipeActionRepository;

        public GetPendingSwipesHandler(ISwipeActionRepository swipeActionRepository)
        {
            _swipeActionRepository = swipeActionRepository;
        }

        public async Task<List<PendingSwipeResponse>> Handle(GetPendingSwipesQuery request, CancellationToken cancellationToken)
        {
            var pendingSwipes = await _swipeActionRepository.GetPendingSwipesByUserIdAsync(request.UserId, cancellationToken);
            return pendingSwipes.Select(sa => new PendingSwipeResponse(sa.Id, sa.SwiperId, sa.CreatedAt)).ToList();
        }
    }
}
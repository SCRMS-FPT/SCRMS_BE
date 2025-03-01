using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.PendingResponses.GetPendingSwipes
{
    public record GetPendingSwipesQuery(Guid UserId) : IRequest<List<PendingSwipeResponse>>;

    public record PendingSwipeResponse(Guid SwiperId, DateTime CreatedAt);

    public class GetPendingSwipesHandler : IRequestHandler<GetPendingSwipesQuery, List<PendingSwipeResponse>>
    {
        private readonly MatchingDbContext _context;

        public GetPendingSwipesHandler(MatchingDbContext context) => _context = context;

        public async Task<List<PendingSwipeResponse>> Handle(GetPendingSwipesQuery request, CancellationToken cancellationToken)
        {
            return await _context.SwipeActions
                .Where(sa => sa.SwipedUserId == request.UserId && sa.Decision == "pending")
                .Select(sa => new PendingSwipeResponse(sa.SwiperId, sa.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
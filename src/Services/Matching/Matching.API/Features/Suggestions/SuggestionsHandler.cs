using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Suggestions
{
    public record GetSuggestionsQuery(int Page, int Limit, Guid UserId) : IRequest<List<UserProfile>>;

    public class GetSuggestionsHandler : IRequestHandler<GetSuggestionsQuery, List<UserProfile>>
    {
        private readonly MatchingDbContext _context;

        public GetSuggestionsHandler(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserProfile>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var swipedUserIds = await _context.SwipeActions
                .Where(sa => sa.SwiperId == userId)
                .Select(sa => sa.SwipedUserId)
                .ToListAsync(cancellationToken);

            var suggestions = await _context.UserSkills
                .Where(u => u.UserId != userId && !swipedUserIds.Contains(u.UserId))
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(u => new UserProfile { Id = u.UserId })
                .ToListAsync(cancellationToken);

            return suggestions;
        }
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
    }
}
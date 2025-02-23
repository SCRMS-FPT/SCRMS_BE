using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Matches.GetMatches
{
    public record GetMatchesQuery(int Page, int Limit, Guid UserId) : IRequest<List<MatchResponse>>;

    public class GetMatchesHandler : IRequestHandler<GetMatchesQuery, List<MatchResponse>>
    {
        private readonly MatchingDbContext _context;

        public GetMatchesHandler(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<List<MatchResponse>> Handle(GetMatchesQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId;
            var matches = await _context.Matches
                .Where(m => m.InitiatorId == userId || m.MatchedUserId == userId)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(m => new MatchResponse
                {
                    Id = m.Id,
                    PartnerId = m.InitiatorId == userId ? m.MatchedUserId : m.InitiatorId,
                    MatchTime = m.MatchTime,
                    Status = m.Status
                })
                .ToListAsync(cancellationToken);

            return matches;
        }
    }

    public class MatchResponse
    {
        public Guid Id { get; set; }
        public Guid PartnerId { get; set; }
        public DateTime MatchTime { get; set; }
        public string Status { get; set; }
    }
}
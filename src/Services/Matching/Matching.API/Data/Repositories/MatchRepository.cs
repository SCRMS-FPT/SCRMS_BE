using Microsoft.EntityFrameworkCore;
using Matching.API.Data.Models;

namespace Matching.API.Data.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly MatchingDbContext _context;

        public MatchRepository(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Match>> GetMatchesByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.Matches
                .Where(m => m.InitiatorId == userId || m.MatchedUserId == userId)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task AddMatchAsync(Match match, CancellationToken cancellationToken)
        {
            await _context.Matches.AddAsync(match, cancellationToken);
        }
    }
}
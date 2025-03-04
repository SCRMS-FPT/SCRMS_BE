using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachSportRepository : ICoachSportRepository
    {
        private readonly CoachDbContext _context;

        public CoachSportRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachSportAsync(CoachSport coachSport, CancellationToken cancellationToken)
        {
            await _context.CoachSports.AddAsync(coachSport, cancellationToken);
        }

        public async Task<List<CoachSport>> GetCoachSportsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachSports.Where(cs => cs.CoachId == coachId).ToListAsync(cancellationToken);
        }

        public async Task DeleteCoachSportAsync(CoachSport coachSport, CancellationToken cancellationToken)
        {
            _context.CoachSports.Remove(coachSport);
            await Task.CompletedTask;
        }
    }
}
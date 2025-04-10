using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data.Repositories
{
    public class CoachRepository : ICoachRepository
    {
        private readonly CoachDbContext _context;

        public CoachRepository(CoachDbContext context)
        {
            _context = context;
        }

        public async Task AddCoachAsync(Models.Coach coach, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(coach.Bio))
            {
                throw new DbUpdateException("Bio is required.");
            }

            await _context.Coaches.AddAsync(coach, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Models.Coach?> GetCoachByIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == coachId && c.Status == "active", cancellationToken);
        }

        public async Task UpdateCoachAsync(Models.Coach coach, CancellationToken cancellationToken)
        {
            _context.Coaches.Update(coach);
            await Task.CompletedTask;
        }

        public async Task<bool> CoachExistsAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.Coaches.AnyAsync(c => c.UserId == coachId, cancellationToken);
        }

        public async Task<List<Models.Coach>> GetAllCoachesAsync(CancellationToken cancellationToken)
        {
            return await _context.Coaches
                .Where(c => c.Status == "active")
                .ToListAsync(cancellationToken);
        }

        public async Task SetCoachStatusAsync(Guid coachId, string status, CancellationToken cancellationToken)
        {
            var coach = await _context.Coaches.FindAsync(new object[] { coachId }, cancellationToken);
            if (coach == null)
                throw new KeyNotFoundException($"Coach with ID {coachId} not found");

            coach.Status = status;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
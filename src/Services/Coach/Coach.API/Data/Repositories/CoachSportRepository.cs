using Coach.API.Data.Models;
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
            var errorMessages = new List<string>();

            // Validate CoachId
            if (coachSport.CoachId == Guid.Empty)
            {
                errorMessages.Add("CoachId is required");
            }

            // Validate SportId
            if (coachSport.SportId == Guid.Empty)
            {
                errorMessages.Add("SportId is required");
            }

            // If there are any validation errors, throw an exception
            if (errorMessages.Any())
            {
                throw new ArgumentException(string.Join(", ", errorMessages));
            }

            // Proceed with the original logic if validation is successful
            await _context.CoachSports.AddAsync(coachSport, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<CoachSport>> GetCoachSportsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.CoachSports.Where(cs => cs.CoachId == coachId).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CoachSport>> GetCoachesBySportIdAsync(Guid sportId, CancellationToken cancellationToken)
        {
            return await _context.CoachSports
                .Where(cs => cs.SportId == sportId)
                .ToListAsync(cancellationToken);
        }
        public async Task DeleteCoachSportAsync(CoachSport coachSport, CancellationToken cancellationToken)
        {
            var existingCoachSport = await _context.CoachSports
        .FindAsync(new object[] { coachSport.CoachId, coachSport.SportId }, cancellationToken);

            if (existingCoachSport == null)
            {
                // If the entity doesn't exist, throw a DbUpdateConcurrencyException
                throw new DbUpdateConcurrencyException("The CoachSport entity could not be found.");
            }
            _context.CoachSports.Remove(coachSport);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
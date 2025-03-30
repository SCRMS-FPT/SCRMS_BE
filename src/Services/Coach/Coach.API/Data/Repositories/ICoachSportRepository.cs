using Coach.API.Data.Models;

namespace Coach.API.Data.Repositories
{
    public interface ICoachSportRepository
    {
        Task AddCoachSportAsync(CoachSport coachSport, CancellationToken cancellationToken);

        Task<List<CoachSport>> GetCoachSportsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken);
        Task<IEnumerable<CoachSport>> GetCoachesBySportIdAsync(Guid sportId, CancellationToken cancellationToken);

        Task DeleteCoachSportAsync(CoachSport coachSport, CancellationToken cancellationToken);
    }
}
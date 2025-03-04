namespace Matching.API.Data.Repositories
{
    public interface IMatchRepository
    {
        Task<List<Match>> GetMatchesByUserIdAsync(Guid userId, int page, int limit, CancellationToken cancellationToken);

        Task AddMatchAsync(Match match, CancellationToken cancellationToken);
    }
}
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Data
{
    public interface IMatchingDbContext
    {
        DbSet<SwipeAction> SwipeActions { get; }
        DbSet<Match> Matches { get; }
        DbSet<UserSkill> UserSkills { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Data
{
    public interface IMatchingDbContext
    {
        DbSet<SwipeAction> SwipeActions { get; }
        DbSet<Match> Matches { get; }
        DbSet<UserSkill> UserSkills { get; }

        DbSet<OutboxMessage> OutboxMessages { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
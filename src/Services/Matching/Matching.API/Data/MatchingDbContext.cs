using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Matching.API.Data
{
    public class MatchingDbContext : DbContext
    {
        public MatchingDbContext(DbContextOptions<MatchingDbContext> options)
            : base(options)
        { }

        public DbSet<SwipeAction> SwipeActions { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.ConfigureOutbox();

            modelBuilder.Entity<SwipeAction>()
                .HasIndex(s => new { s.SwiperId, s.SwipedUserId });

            modelBuilder.Entity<UserSkill>()
                .HasKey(us => new { us.UserId, us.SportId });
        }
    }
}
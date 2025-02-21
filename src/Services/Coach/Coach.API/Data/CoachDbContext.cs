using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Data
{
    public class CoachDbContext : DbContext
    {
        public CoachDbContext(DbContextOptions<CoachDbContext> options) : base(options)
        {
        }

        public DbSet<Models.Coach> Coaches => Set<Models.Coach>();
        public DbSet<CoachSchedule> CoachSchedules => Set<CoachSchedule>();
        public DbSet<CoachBooking> CoachBookings => Set<CoachBooking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Coach>(entity =>
            {
                entity.HasKey(c => c.UserId);
                entity.Property(c => c.Bio).HasMaxLength(1000);
                entity.Property(c => c.RatePerHour).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<CoachSchedule>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne<Models.Coach>()
                    .WithMany()
                    .HasForeignKey(s => s.CoachId);
            });

            modelBuilder.Entity<CoachBooking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Status).HasMaxLength(50);
                entity.HasOne<Models.Coach>()
                    .WithMany()
                    .HasForeignKey(b => b.CoachId);
            });
        }
    }
}
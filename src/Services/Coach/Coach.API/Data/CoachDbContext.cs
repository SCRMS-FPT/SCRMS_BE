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
        public DbSet<CoachSport> CoachSports => Set<CoachSport>();
        public DbSet<CoachPackage> CoachPackages => Set<CoachPackage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Coach>(entity =>
            {
                entity.HasKey(c => c.UserId);
                entity.Property(c => c.Bio).HasMaxLength(1000);
                entity.Property(c => c.RatePerHour).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<CoachSport>(entity =>
            {
                entity.HasKey(cs => new { cs.CoachId, cs.SportId });

                entity.HasOne(cs => cs.Coach)
                    .WithMany(c => c.Sports)
                    .HasForeignKey(cs => cs.CoachId);
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
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne<Models.Coach>()
                    .WithMany()
                    .HasForeignKey(b => b.CoachId);
                entity.HasOne<CoachPackage>()
                    .WithMany()
                    .HasForeignKey(b => b.PackageId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<CoachPackage>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(255);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.HasOne<Models.Coach>()
                    .WithMany()
                    .HasForeignKey(p => p.CoachId);
            });
        }
    }
}
using System.Collections.Generic;
using System.Reflection.Emit;
using Coach.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.Extensions;

namespace Coach.API.Data
{
    public class CoachDbContext : DbContext
    {
        public CoachDbContext()
        { }

        public CoachDbContext(DbContextOptions<CoachDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Models.Coach> Coaches => Set<Models.Coach>();
        public virtual DbSet<CoachSchedule> CoachSchedules => Set<CoachSchedule>();
        public virtual DbSet<CoachBooking> CoachBookings => Set<CoachBooking>();
        public virtual DbSet<CoachSport> CoachSports => Set<CoachSport>();
        public virtual DbSet<CoachPackage> CoachPackages => Set<CoachPackage>();
        public virtual DbSet<CoachPackagePurchase> CoachPackagePurchases { get; set; }
        public virtual DbSet<CoachPromotion> CoachPromotions { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Coach>(entity =>
            {
                entity.HasKey(c => c.UserId);
                entity.Property(c => c.Bio).HasMaxLength(1000);
                entity.Property(c => c.RatePerHour).HasColumnType("decimal(18,2)");
                entity.Property(c => c.Status).HasMaxLength(20).HasDefaultValue("active"); // Thêm cấu hình này
            });

            modelBuilder.Entity<CoachSport>(entity =>
            {
                entity.HasKey(cs => new { cs.CoachId, cs.SportId });

                entity.HasOne(cs => cs.Coach)
                    .WithMany(c => c.CoachSports)
                    .HasForeignKey(cs => cs.CoachId);
            });

            modelBuilder.Entity<CoachSchedule>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Coach)
                    .WithMany(c => c.Schedules)
                    .HasForeignKey(s => s.CoachId);
            });

            modelBuilder.Entity<CoachBooking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Status).HasMaxLength(50);
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(cb => cb.Coach)
                    .WithMany(c => c.Bookings)
                    .HasForeignKey(cb => cb.CoachId);
                entity.HasOne(cb => cb.Package)
                    .WithMany(p => p.Bookings) // Chỉ định navigation trong CoachPackage
                    .HasForeignKey(cb => cb.PackageId)
                    .IsRequired(false);
                entity.Property(cb => cb.Status)
                    .HasMaxLength(20);
                entity.HasIndex(cb => cb.SportId);
            });

            modelBuilder.Entity<CoachPackage>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(255);
                entity.Property(p => p.Description).HasMaxLength(1000);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("active");
                entity.HasOne(cp => cp.Coach)
                    .WithMany(c => c.Packages)
                    .HasForeignKey(cp => cp.CoachId);
            });

            modelBuilder.Entity<CoachPackagePurchase>()
           .HasKey(cpp => cpp.Id);
            modelBuilder.Entity<CoachPackagePurchase>()
                .HasOne(cpp => cpp.CoachPackage)
                .WithMany(cp => cp.Purchases)
                .HasForeignKey(cpp => cpp.CoachPackageId);

            // **CoachPromotion**
            modelBuilder.Entity<CoachPromotion>()
                .HasKey(cp => cp.Id);
            modelBuilder.Entity<CoachPromotion>()
                .HasOne(cp => cp.Coach)
                .WithMany(c => c.Promotions)
                .HasForeignKey(cp => cp.CoachId);
            modelBuilder.Entity<CoachPromotion>()
                .Property(cp => cp.DiscountType)
                .HasMaxLength(50);
            modelBuilder.Entity<CoachPromotion>()
                .HasOne(cp => cp.Package)
                .WithMany(p => p.Promotions) // You'll need to add this collection to CoachPackage
                .HasForeignKey(cp => cp.PackageId)
                .IsRequired(false);

            // Configure outbox
            modelBuilder.ConfigureOutbox();
        }
    }
}
using Identity.Application.Data;
using Identity.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Data
{
    public class IdentityDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<ServicePackageSubscription> Subscriptions { get; set; }

        public DbSet<ServicePackagePromotion> ServicePackagePromotions { get; set; }

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(b =>
            {
                b.Property(u => u.FirstName).HasMaxLength(255).IsRequired();
                b.Property(u => u.LastName).HasMaxLength(255).IsRequired();
                b.Property(u => u.BirthDate).IsRequired();
                b.Property(u => u.Gender).HasMaxLength(50).IsRequired();
            });

            builder.Entity<ServicePackage>(b =>
            {
                b.Property(p => p.Name).HasMaxLength(255).IsRequired();
                b.Property(p => p.Price).HasPrecision(18, 2);
            });

            builder.Entity<ServicePackageSubscription>()
                .HasOne(s => s.Package)
                .WithMany()
                .HasForeignKey(s => s.PackageId);

            builder.Entity<ServicePackageSubscription>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            builder.Entity<ServicePackagePromotion>()
                .HasOne(p => p.ServicePackage)
                .WithMany(sp => sp.Promotions) // Giả sử ServicePackage có thuộc tính Promotions
                .HasForeignKey(p => p.ServicePackageId);

            // Cấu hình giá trị mặc định cho CreatedAt và UpdatedAt
            builder.Entity<ServicePackagePromotion>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("NOW()");

            builder.Entity<ServicePackagePromotion>()
                .Property(p => p.UpdatedAt)
                .HasDefaultValueSql("NOW()");
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
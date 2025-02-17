using Identity.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Identity.Application.Data;

namespace Identity.Infrastructure.Data
{
    public class IdentityDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<ServicePackageSubscription> Subscriptions { get; set; }

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
        }
    }
}

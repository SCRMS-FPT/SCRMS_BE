using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Data
{
    public class ReviewDbContext : DbContext
    {
        public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
        {
        }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewFlag> ReviewFlags { get; set; }
        public DbSet<ReviewReply> ReviewReplies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.SubjectType, r.SubjectId });

            // Quan hệ Review - ReviewReply
            modelBuilder.Entity<Review>()
                .HasMany(r => r.Replies)
                .WithOne(rr => rr.Review)
                .HasForeignKey(rr => rr.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Review - ReviewFlag
            modelBuilder.Entity<Review>()
                .HasMany(r => r.Flags)
                .WithOne(rf => rf.Review)
                .HasForeignKey(rf => rf.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
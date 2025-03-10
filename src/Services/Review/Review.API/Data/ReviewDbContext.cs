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
        }
    }
}
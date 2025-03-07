using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Data
{
    public interface IReviewDbContext
    {
        DbSet<Review> Reviews { get; set; }
        DbSet<ReviewFlag> ReviewFlags { get; set; }
        DbSet<ReviewReply> ReviewReplies { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

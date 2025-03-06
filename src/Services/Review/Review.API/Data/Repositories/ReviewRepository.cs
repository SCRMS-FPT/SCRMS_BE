using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Models;

namespace Reviews.API.Data.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ReviewDbContext _context;

        public ReviewRepository(ReviewDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(Review review, CancellationToken cancellationToken)
        {
            await _context.Reviews.AddAsync(review, cancellationToken);
        }

        public async Task<Review?> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
        }

        public Task RemoveReviewAsync(Review review, CancellationToken cancellationToken)
        {
            _context.Reviews.Remove(review);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Review>> GetReviewsBySubjectAsync(string subjectType, Guid subjectId, int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.SubjectType == subjectType && r.SubjectId == subjectId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ReviewReply>> GetReviewRepliesAsync(Guid reviewId, int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.ReviewReplies
                .Where(r => r.ReviewId == reviewId)
                .OrderBy(r => r.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Review>> GetReviewsByCoachIdAsync(Guid coachId, int page, int limit, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.SubjectType == "coach" && r.SubjectId == coachId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task AddReviewFlagAsync(ReviewFlag flag, CancellationToken cancellationToken)
        {
            await _context.ReviewFlags.AddAsync(flag, cancellationToken);
        }

        public async Task AddReviewReplyAsync(ReviewReply reply, CancellationToken cancellationToken)
        {
            await _context.ReviewReplies.AddAsync(reply, cancellationToken);
        }
    }
}
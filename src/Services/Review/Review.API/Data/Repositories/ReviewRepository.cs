using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Data.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IReviewDbContext _context;

        public ReviewRepository(IReviewDbContext context)
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
                .Include(r => r.Replies) // Eager load replies
                .Where(r => r.SubjectType == "coach" && r.SubjectId == coachId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync(cancellationToken);
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

        public async Task AddReviewFlagAsync(ReviewFlag flag, CancellationToken cancellationToken)
        {
            await _context.ReviewFlags.AddAsync(flag, cancellationToken);
        }

        public async Task AddReviewReplyAsync(ReviewReply reply, CancellationToken cancellationToken)
        {
            await _context.ReviewReplies.AddAsync(reply, cancellationToken);
        }

        public async Task<int> CountReviewsBySubjectAsync(string subjectType, Guid subjectId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .CountAsync(r => r.SubjectType == subjectType && r.SubjectId == subjectId, cancellationToken);
        }

        public async Task<int> CountReviewRepliesAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            return await _context.ReviewReplies
                .CountAsync(r => r.ReviewId == reviewId, cancellationToken);
        }

        public async Task<int> CountReviewsByCoachIdAsync(Guid coachId, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .CountAsync(r => r.SubjectType == "coach" && r.SubjectId == coachId, cancellationToken);
        }

        public async Task<int> CountReviewsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = _context.Reviews.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Lấy đến hết ngày kết thúc
                var endDateWithTime = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(r => r.CreatedAt <= endDateWithTime);
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<int> CountFlaggedReviewsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var query = _context.ReviewFlags
                .Select(rf => rf.ReviewId)
                .Distinct()
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(reviewId =>
                    _context.ReviewFlags.Any(rf =>
                        rf.ReviewId == reviewId &&
                        rf.CreatedAt >= startDate.Value));
            }

            if (endDate.HasValue)
            {
                var endDateWithTime = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(reviewId =>
                    _context.ReviewFlags.Any(rf =>
                        rf.ReviewId == reviewId &&
                        rf.CreatedAt <= endDateWithTime));
            }

            return await query.CountAsync(cancellationToken);
        }
    }
}
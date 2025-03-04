namespace Reviews.API.Data.Repositories
{
    public interface IReviewRepository
    {
        Task AddReviewAsync(Review review, CancellationToken cancellationToken);

        Task<Review?> GetReviewByIdAsync(Guid reviewId, CancellationToken cancellationToken);

        Task RemoveReviewAsync(Review review, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);

        Task<List<Review>> GetReviewsBySubjectAsync(string subjectType, Guid subjectId, int page, int limit, CancellationToken cancellationToken);

        Task<List<ReviewReply>> GetReviewRepliesAsync(Guid reviewId, int page, int limit, CancellationToken cancellationToken);

        Task<List<Review>> GetReviewsByCoachIdAsync(Guid coachId, int page, int limit, CancellationToken cancellationToken);

        Task AddReviewFlagAsync(ReviewFlag flag, CancellationToken cancellationToken);

        Task AddReviewReplyAsync(ReviewReply reply, CancellationToken cancellationToken);
    }
}
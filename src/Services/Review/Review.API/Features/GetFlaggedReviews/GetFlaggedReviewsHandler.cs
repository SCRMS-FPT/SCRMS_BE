using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;
using Reviews.API.Features.GetReviews;

namespace Reviews.API.Features.GetFlaggedReviews
{
    public record GetFlaggedReviewsQuery(int Page, int Limit) : IRequest<PaginatedResult<FlaggedReviewResponse>>;

    public record FlaggedReviewResponse(
        Guid Id,
        Guid ReviewerId,
        string SubjectType,
        Guid SubjectId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        int FlagCount,
        List<ReviewReplyResponse> Replies);

    public class GetFlaggedReviewsHandler : IRequestHandler<GetFlaggedReviewsQuery, PaginatedResult<FlaggedReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ReviewDbContext _dbContext;

        public GetFlaggedReviewsHandler(IReviewRepository reviewRepository, ReviewDbContext dbContext)
        {
            _reviewRepository = reviewRepository;
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<FlaggedReviewResponse>> Handle(GetFlaggedReviewsQuery request, CancellationToken cancellationToken)
        {
            // Lấy ID của các review bị flag
            var flaggedReviewIds = await _dbContext.ReviewFlags
                .Select(rf => rf.ReviewId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Đếm tổng số review bị flag
            var totalCount = flaggedReviewIds.Count;

            // Lấy danh sách các review bị flag với phân trang
            var flaggedReviews = await _dbContext.Reviews
                .Include(r => r.Replies)
                .Where(r => flaggedReviewIds.Contains(r.Id))
                .OrderByDescending(r =>
                    _dbContext.ReviewFlags.Count(rf => rf.ReviewId == r.Id))
                .ThenByDescending(r => r.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            // Tạo danh sách kết quả với số lượng flag cho mỗi review
            var flaggedReviewResponses = new List<FlaggedReviewResponse>();

            foreach (var review in flaggedReviews)
            {
                // Đếm số lượng flag cho review hiện tại
                var flagCount = await _dbContext.ReviewFlags
                    .CountAsync(rf => rf.ReviewId == review.Id, cancellationToken);

                // Tạo đối tượng response
                var response = new FlaggedReviewResponse(
                    review.Id,
                    review.ReviewerId,
                    review.SubjectType,
                    review.SubjectId,
                    review.Rating,
                    review.Comment,
                    review.CreatedAt,
                    flagCount,
                    review.Replies.Select(reply => new ReviewReplyResponse(
                        reply.Id,
                        reply.ResponderId,
                        reply.ReplyText,
                        reply.CreatedAt)).ToList()
                );

                flaggedReviewResponses.Add(response);
            }

            return new PaginatedResult<FlaggedReviewResponse>(
                request.Page,
                request.Limit,
                totalCount,
                flaggedReviewResponses
            );
        }
    }
}
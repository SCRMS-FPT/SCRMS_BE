using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Models;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.GetFlaggedReviews
{
    public record GetFlaggedReviewsQuery(int Page, int Limit, string? Status) : IRequest<PaginatedResult<ReviewFlagResponse>>;

    public record ReviewFlagResponse(
        Guid Id,
        Guid ReviewId,
        Guid ReportedBy,
        string FlagReason,
        string Status,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        // Thông tin bổ sung về review bị báo cáo
        ReviewSummary Review
    );

    public record ReviewSummary(
        Guid Id,
        Guid ReviewerId,
        string SubjectType,
        Guid SubjectId,
        int Rating,
        string? Comment,
        DateTime CreatedAt
    );

    public class GetFlaggedReviewsHandler : IRequestHandler<GetFlaggedReviewsQuery, PaginatedResult<ReviewFlagResponse>>
    {
        private readonly ReviewDbContext _dbContext;

        public GetFlaggedReviewsHandler(ReviewDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedResult<ReviewFlagResponse>> Handle(GetFlaggedReviewsQuery request, CancellationToken cancellationToken)
        {
            // Bắt đầu truy vấn từ ReviewFlags
            var query = _dbContext.ReviewFlags.AsQueryable();

            // Filter theo status nếu được cung cấp
            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(rf => rf.Status == request.Status);
            }

            // Đếm tổng số flag
            var totalCount = await query.CountAsync(cancellationToken);

            // Lấy danh sách các flag với phân trang
            var flags = await query
                .OrderByDescending(rf => rf.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            // Lấy thông tin về các review bị báo cáo
            var reviewIds = flags.Select(f => f.ReviewId).ToList();
            var reviews = await _dbContext.Reviews
                .Where(r => reviewIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, cancellationToken);

            // Tạo response
            var flagResponses = flags.Select(flag =>
            {
                // Kiểm tra xem review có tồn tại không
                var hasReview = reviews.TryGetValue(flag.ReviewId, out var review);

                var reviewSummary = hasReview
                    ? new ReviewSummary(
                        review!.Id,
                        review.ReviewerId,
                        review.SubjectType,
                        review.SubjectId,
                        review.Rating,
                        review.Comment,
                        review.CreatedAt)
                    : new ReviewSummary(
                        Guid.Empty,
                        Guid.Empty,
                        "Unknown",
                        Guid.Empty,
                        0,
                        "Review not found or deleted",
                        DateTime.MinValue);

                return new ReviewFlagResponse(
                    flag.Id,
                    flag.ReviewId,
                    flag.ReportedBy,
                    flag.FlagReason,
                    flag.Status,
                    flag.CreatedAt,
                    flag.UpdatedAt,
                    reviewSummary
                );
            }).ToList();

            return new PaginatedResult<ReviewFlagResponse>(
                request.Page,
                request.Limit,
                totalCount,
                flagResponses
            );
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data;

namespace Reviews.API.Features.GetReviewStats
{
    public class GetReviewStatsHandler : IRequestHandler<GetReviewStatsQuery, GetReviewStatsResponse>
    {
        private readonly ReviewDbContext _dbContext;

        public GetReviewStatsHandler(ReviewDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetReviewStatsResponse> Handle(GetReviewStatsQuery request, CancellationToken cancellationToken)
        {
            // Xử lý đếm tổng số đánh giá
            var reviewsQuery = _dbContext.Reviews.AsQueryable();

            if (request.StartDate.HasValue)
            {
                reviewsQuery = reviewsQuery.Where(r => r.CreatedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endDateWithTime = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                reviewsQuery = reviewsQuery.Where(r => r.CreatedAt <= endDateWithTime);
            }

            var totalReviews = await reviewsQuery.CountAsync(cancellationToken);

            // Xử lý đếm số đánh giá bị báo cáo (đã được flag)
            var reportedReviewsQuery = _dbContext.ReviewFlags
                .Select(rf => rf.ReviewId)
                .Distinct()
                .AsQueryable();

            if (request.StartDate.HasValue)
            {
                reportedReviewsQuery = reportedReviewsQuery.Where(reviewId =>
                    _dbContext.ReviewFlags.Any(rf =>
                        rf.ReviewId == reviewId &&
                        rf.CreatedAt >= request.StartDate.Value));
            }

            if (request.EndDate.HasValue)
            {
                var endDateWithTime = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                reportedReviewsQuery = reportedReviewsQuery.Where(reviewId =>
                    _dbContext.ReviewFlags.Any(rf =>
                        rf.ReviewId == reviewId &&
                        rf.CreatedAt <= endDateWithTime));
            }

            var reportedReviews = await reportedReviewsQuery.CountAsync(cancellationToken);

            // Tạo response
            return new GetReviewStatsResponse
            {
                TotalReviews = totalReviews,
                ReportedReviews = reportedReviews,
                DateRange = new DateRangeDto
                {
                    StartDate = request.StartDate?.ToString("yyyy-MM-dd"),
                    EndDate = request.EndDate?.ToString("yyyy-MM-dd")
                }
            };
        }
    }
}
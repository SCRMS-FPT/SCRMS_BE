using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.GetReviewDetail
{
    public record GetReviewDetailQuery(Guid ReviewId) : IRequest<ReviewDetailResponse>;

    public record ReviewDetailResponse(Guid Id, Guid ReviewerId, string SubjectType, Guid SubjectId, int Rating, string? Comment, DateTime CreatedAt);

    public class GetReviewDetailHandler : IRequestHandler<GetReviewDetailQuery, ReviewDetailResponse>
    {
        private readonly ReviewDbContext _context;

        public GetReviewDetailHandler(ReviewDbContext context) => _context = context;

        public async Task<ReviewDetailResponse> Handle(GetReviewDetailQuery request, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
                ?? throw new Exception("Review not found");

            return new ReviewDetailResponse(review.Id, review.ReviewerId, review.SubjectType, review.SubjectId, review.Rating, review.Comment, review.CreatedAt);
        }
    }
}
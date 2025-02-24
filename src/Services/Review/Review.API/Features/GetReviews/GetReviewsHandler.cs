using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.GetReviews
{
    public record GetReviewsQuery(string SubjectType, Guid SubjectId, int Page, int Limit) : IRequest<List<ReviewResponse>>;

    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt);

    public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, List<ReviewResponse>>
    {
        private readonly ReviewDbContext _context;

        public GetReviewsHandler(ReviewDbContext context) => _context = context;

        public async Task<List<ReviewResponse>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.SubjectType == request.SubjectType && r.SubjectId == request.SubjectId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(r => new ReviewResponse(r.Id, r.ReviewerId, r.Rating, r.Comment, r.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
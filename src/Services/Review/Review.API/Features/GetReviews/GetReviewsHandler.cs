using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.GetReviews
{
    public record GetReviewsQuery(string SubjectType, Guid SubjectId, int Page, int Limit) : IRequest<List<ReviewResponse>>;

    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt);

    public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, List<ReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewsHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewResponse>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetReviewsBySubjectAsync(request.SubjectType, request.SubjectId, request.Page, request.Limit, cancellationToken);
            return reviews.Select(r => new ReviewResponse(r.Id, r.ReviewerId, r.Rating, r.Comment, r.CreatedAt)).ToList();
        }
    }
}
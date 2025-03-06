using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.GetSelfReviews
{
    public record GetSelfReviewsQuery(Guid coachId, int Page, int Limit) : IRequest<List<ReviewResponse>>;

    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt);

    public class GetSelfReviewsQueryValidator : AbstractValidator<GetSelfReviewsQuery>
    {
        public GetSelfReviewsQueryValidator()
        {
            RuleFor(x => x.coachId)
                .NotEmpty().WithMessage("CoachId is required.");
        }
    }

    public class GetSelfReviewsHandler : IRequestHandler<GetSelfReviewsQuery, List<ReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetSelfReviewsHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewResponse>> Handle(GetSelfReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetReviewsByCoachIdAsync(request.coachId, request.Page, request.Limit, cancellationToken);
            return reviews.Select(r => new ReviewResponse(r.Id, r.ReviewerId, r.Rating, r.Comment, r.CreatedAt)).ToList();
        }
    }
}
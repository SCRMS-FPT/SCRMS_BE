using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;

namespace Reviews.API.Features.GetSelfReviews
{
    public record GetSelfReviewsQuery(Guid CoachId, int Page, int Limit) : IRequest<PaginatedResult<ReviewResponse>>;

    public record ReviewResponse(
        Guid Id,
        Guid ReviewerId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        List<ReviewReplyResponse> Replies
    );

    public class GetSelfReviewsQueryValidator : AbstractValidator<GetSelfReviewsQuery>
    {
        public GetSelfReviewsQueryValidator()
        {
            RuleFor(x => x.CoachId)
                .NotEmpty().WithMessage("CoachId is required.");
        }
    }

    public class GetSelfReviewsHandler : IRequestHandler<GetSelfReviewsQuery, PaginatedResult<ReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetSelfReviewsHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PaginatedResult<ReviewResponse>> Handle(GetSelfReviewsQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await _reviewRepository.CountReviewsByCoachIdAsync(request.CoachId, cancellationToken);
            var reviews = await _reviewRepository.GetReviewsByCoachIdAsync(request.CoachId, request.Page, request.Limit, cancellationToken);
            var reviewResponses = reviews.Select(r => new ReviewResponse(
                r.Id,
                r.ReviewerId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.Replies.Select(reply => new ReviewReplyResponse(reply.Id, reply.ResponderId, reply.ReplyText, reply.CreatedAt)).ToList()
            )).ToList();

            return new PaginatedResult<ReviewResponse>(request.Page,
                request.Limit,
                totalCount,
                 reviewResponses
            );
        }
    }
}
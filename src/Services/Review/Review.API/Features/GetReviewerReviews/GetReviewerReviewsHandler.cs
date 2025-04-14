using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;

namespace Reviews.API.Features.GetReviewerReviews
{
    public record GetReviewerReviewsQuery(Guid ReviewerId, int Page, int Limit) : IRequest<PaginatedResult<ReviewerReviewResponse>>;

    public record ReviewerReviewResponse(
        Guid Id,
        string SubjectType,
        Guid SubjectId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        List<ReviewReplyResponse> Replies
    );

    public class GetReviewerReviewsQueryValidator : AbstractValidator<GetReviewerReviewsQuery>
    {
        public GetReviewerReviewsQueryValidator()
        {
            RuleFor(x => x.ReviewerId)
                .NotEmpty().WithMessage("ReviewerId is required.");
        }
    }

    public class GetReviewerReviewsHandler : IRequestHandler<GetReviewerReviewsQuery, PaginatedResult<ReviewerReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewerReviewsHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PaginatedResult<ReviewerReviewResponse>> Handle(GetReviewerReviewsQuery request, CancellationToken cancellationToken)
        {
            // Get the total count of reviews by the specified reviewer
            var totalCount = await _reviewRepository.CountReviewsByReviewerIdAsync(request.ReviewerId, cancellationToken);

            // Get the paginated list of reviews
            var reviews = await _reviewRepository.GetReviewsByReviewerIdAsync(request.ReviewerId, request.Page, request.Limit, cancellationToken);

            // Map the domain entities to response DTOs
            var reviewResponses = reviews.Select(r => new ReviewerReviewResponse(
                r.Id,
                r.SubjectType,
                r.SubjectId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.Replies.Select(reply => new ReviewReplyResponse(reply.Id, reply.ResponderId, reply.ReplyText, reply.CreatedAt)).ToList()
            )).ToList();

            // Return the paginated result
            return new PaginatedResult<ReviewerReviewResponse>(
                request.Page,
                request.Limit,
                totalCount,
                reviewResponses
            );
        }
    }
}
using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;

namespace Reviews.API.Features.GetReviews
{
    public record GetReviewsQuery(string SubjectType, Guid SubjectId, int Page, int Limit) : IRequest<PaginatedResult<ReviewResponse>>;

    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt, List<ReviewReplyResponse> ReviewReplyResponses);

    public class GetReviewsHandler : IRequestHandler<GetReviewsQuery, PaginatedResult<ReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewsHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PaginatedResult<ReviewResponse>> Handle(GetReviewsQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await _reviewRepository.CountReviewsBySubjectAsync(request.SubjectType, request.SubjectId, cancellationToken);
            var reviews = await _reviewRepository.GetReviewsBySubjectAsync(request.SubjectType, request.SubjectId, request.Page, request.Limit, cancellationToken);
            var reviewResponses = reviews.Select(r => new ReviewResponse(
                r.Id,
                r.ReviewerId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.Replies.Select(reply => new ReviewReplyResponse(reply.Id, reply.ResponderId, reply.ReplyText, reply.CreatedAt)).ToList()
            )).ToList();

            return new PaginatedResult<ReviewResponse>
            (
                 request.Page,
                request.Limit,
                 totalCount,
                reviewResponses
            );
        }
    }
}
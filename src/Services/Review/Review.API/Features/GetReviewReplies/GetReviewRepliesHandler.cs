using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.GetReviewReplies
{
    public record GetReviewRepliesQuery(Guid ReviewId, int Page, int Limit) : IRequest<PaginatedResult<ReviewReplyResponse>>;
    public record ReviewReplyResponse(Guid Id, Guid ResponderId, string ReplyText, DateTime CreatedAt);

    public class GetReviewRepliesHandler : IRequestHandler<GetReviewRepliesQuery, PaginatedResult<ReviewReplyResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewRepliesHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PaginatedResult<ReviewReplyResponse>> Handle(GetReviewRepliesQuery request, CancellationToken cancellationToken)
        {
            var totalCount = await _reviewRepository.CountReviewRepliesAsync(request.ReviewId, cancellationToken);
            var replies = await _reviewRepository.GetReviewRepliesAsync(request.ReviewId, request.Page, request.Limit, cancellationToken);
            var replyResponses = replies.Select(r => new ReviewReplyResponse(r.Id, r.ResponderId, r.ReplyText, r.CreatedAt)).ToList();

            return new PaginatedResult<ReviewReplyResponse>(request.Page,
                request.Limit,
                totalCount,
                 replyResponses
            );
        }
    }
}
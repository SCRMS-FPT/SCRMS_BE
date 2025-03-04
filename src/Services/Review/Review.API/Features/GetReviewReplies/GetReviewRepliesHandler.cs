using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.GetReviewReplies
{
    public record GetReviewRepliesQuery(Guid ReviewId, int Page, int Limit) : IRequest<List<ReviewReplyResponse>>;

    public record ReviewReplyResponse(Guid Id, Guid ResponderId, string ReplyText, DateTime CreatedAt);

    public class GetReviewRepliesHandler : IRequestHandler<GetReviewRepliesQuery, List<ReviewReplyResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewRepliesHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<List<ReviewReplyResponse>> Handle(GetReviewRepliesQuery request, CancellationToken cancellationToken)
        {
            var replies = await _reviewRepository.GetReviewRepliesAsync(request.ReviewId, request.Page, request.Limit, cancellationToken);
            return replies.Select(r => new ReviewReplyResponse(r.Id, r.ResponderId, r.ReplyText, r.CreatedAt)).ToList();
        }
    }
}
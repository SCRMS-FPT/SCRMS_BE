using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.GetReviewReplies
{
    public record GetReviewRepliesQuery(Guid ReviewId, int Page, int Limit) : IRequest<List<ReviewReplyResponse>>;

    public record ReviewReplyResponse(Guid Id, Guid ResponderId, string ReplyText, DateTime CreatedAt);

    public class GetReviewRepliesHandler : IRequestHandler<GetReviewRepliesQuery, List<ReviewReplyResponse>>
    {
        private readonly ReviewDbContext _context;

        public GetReviewRepliesHandler(ReviewDbContext context) => _context = context;

        public async Task<List<ReviewReplyResponse>> Handle(GetReviewRepliesQuery request, CancellationToken cancellationToken)
        {
            return await _context.ReviewReplies
                .Where(r => r.ReviewId == request.ReviewId)
                .OrderBy(r => r.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(r => new ReviewReplyResponse(r.Id, r.ResponderId, r.ReplyText, r.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}
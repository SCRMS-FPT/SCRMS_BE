namespace Reviews.API.Features.ReplyToReview
{
    public record ReplyToReviewCommand(Guid ReviewId, Guid ResponderId, string ReplyText) : IRequest<Guid>;

    public class ReplyToReviewHandler : IRequestHandler<ReplyToReviewCommand, Guid>
    {
        private readonly ReviewDbContext _context;

        public ReplyToReviewHandler(ReviewDbContext context) => _context = context;

        public async Task<Guid> Handle(ReplyToReviewCommand request, CancellationToken cancellationToken)
        {
            var reply = new ReviewReply
            {
                Id = Guid.NewGuid(),
                ReviewId = request.ReviewId,
                ResponderId = request.ResponderId,
                ReplyText = request.ReplyText,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ReviewReplies.Add(reply);
            await _context.SaveChangesAsync(cancellationToken);
            return reply.Id;
        }
    }
}
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.ReplyToReview
{
    public record ReplyToReviewCommand(Guid ReviewId, Guid ResponderId, string ReplyText) : IRequest<Guid>;

    public class ReplyToReviewHandler : IRequestHandler<ReplyToReviewCommand, Guid>
    {
        private readonly IReviewRepository _reviewRepository;

        public ReplyToReviewHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

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

            await _reviewRepository.AddReviewReplyAsync(reply, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
            return reply.Id;
        }
    }
}
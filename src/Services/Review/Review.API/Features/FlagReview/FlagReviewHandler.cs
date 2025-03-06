using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.FlagReview
{
    public record FlagReviewCommand(Guid ReviewId, Guid ReportedBy, string FlagReason) : IRequest<Guid>;

    public class FlagReviewHandler : IRequestHandler<FlagReviewCommand, Guid>
    {
        private readonly IReviewRepository _reviewRepository;

        public FlagReviewHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Guid> Handle(FlagReviewCommand request, CancellationToken cancellationToken)
        {
            var flag = new ReviewFlag
            {
                Id = Guid.NewGuid(),
                ReviewId = request.ReviewId,
                ReportedBy = request.ReportedBy,
                FlagReason = request.FlagReason,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddReviewFlagAsync(flag, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
            return flag.Id;
        }
    }
}
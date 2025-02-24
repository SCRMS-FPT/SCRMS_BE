namespace Reviews.API.Features.FlagReview
{
    public record FlagReviewCommand(Guid ReviewId, Guid ReportedBy, string FlagReason) : IRequest<Guid>;

    public class FlagReviewHandler : IRequestHandler<FlagReviewCommand, Guid>
    {
        private readonly ReviewDbContext _context;

        public FlagReviewHandler(ReviewDbContext context) => _context = context;

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

            _context.ReviewFlags.Add(flag);
            await _context.SaveChangesAsync(cancellationToken);
            return flag.Id;
        }
    }
}
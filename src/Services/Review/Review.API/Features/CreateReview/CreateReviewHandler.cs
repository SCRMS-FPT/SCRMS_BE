namespace Reviews.API.Features.CreateReview
{
    public record CreateReviewCommand(Guid ReviewerId, string SubjectType, Guid SubjectId, int Rating, string? Comment) : IRequest<Guid>;

    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Guid>
    {
        private readonly ReviewDbContext _context;

        public CreateReviewHandler(ReviewDbContext context) => _context = context;

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");
            if (!new[] { "court", "coach" }.Contains(request.SubjectType))
                throw new ArgumentException("Invalid subject type.");

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = request.ReviewerId,
                SubjectType = request.SubjectType,
                SubjectId = request.SubjectId,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync(cancellationToken);
            return review.Id;
        }
    }
}
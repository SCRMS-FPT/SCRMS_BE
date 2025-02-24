using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.UpdateReview
{
    public record UpdateReviewCommand(Guid ReviewId, Guid UserId, int Rating, string? Comment) : IRequest;

    public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand>
    {
        private readonly ReviewDbContext _context;

        public UpdateReviewHandler(ReviewDbContext context) => _context = context;

        public async Task Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.ReviewerId == request.UserId, cancellationToken)
                ?? throw new Exception("Review not found or unauthorized");

            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.DeleteReview
{
    public record DeleteReviewCommand(Guid ReviewId, Guid UserId) : IRequest;

    public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
    {
        private readonly ReviewDbContext _context;

        public DeleteReviewHandler(ReviewDbContext context) => _context = context;

        public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == request.ReviewId && r.ReviewerId == request.UserId, cancellationToken)
                ?? throw new Exception("Review not found or unauthorized");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
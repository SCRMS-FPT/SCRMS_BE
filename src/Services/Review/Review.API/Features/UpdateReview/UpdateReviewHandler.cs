using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.UpdateReview
{
    public record UpdateReviewCommand(Guid ReviewId, Guid UserId, int Rating, string? Comment) : IRequest;

    public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand>
    {
        private readonly IReviewRepository _reviewRepository;

        public UpdateReviewHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
            if (review == null || review.ReviewerId != request.UserId)
                throw new Exception("Review not found or unauthorized");

            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            review.UpdatedAt = DateTime.UtcNow;
            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
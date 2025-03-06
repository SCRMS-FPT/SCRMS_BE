using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.DeleteReview
{
    public record DeleteReviewCommand(Guid ReviewId, Guid UserId) : IRequest;

    public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
    {
        private readonly IReviewRepository _reviewRepository;

        public DeleteReviewHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
            if (review == null || review.ReviewerId != request.UserId)
                throw new Exception("Review not found or unauthorized");

            await _reviewRepository.RemoveReviewAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
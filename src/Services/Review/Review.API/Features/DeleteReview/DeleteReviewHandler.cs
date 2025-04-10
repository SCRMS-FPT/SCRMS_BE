using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.DeleteReview
{
    public record DeleteReviewCommand(Guid ReviewId, Guid UserId, bool IsAdmin) : IRequest;

    public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<DeleteReviewHandler> _logger;

        public DeleteReviewHandler(IReviewRepository reviewRepository, ILogger<DeleteReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
            if (review == null)
                throw new ArgumentException("Review not found");

            // Nếu là admin hoặc chính người tạo review, cho phép xóa
            if (request.IsAdmin || review.ReviewerId == request.UserId)
            {
                // Ghi log nếu admin xóa review của người khác
                if (request.IsAdmin && review.ReviewerId != request.UserId)
                {
                    _logger.LogInformation("Admin {UserId} deleted review {ReviewId} created by user {ReviewerId}",
                        request.UserId, request.ReviewId, review.ReviewerId);
                }

                await _reviewRepository.RemoveReviewAsync(review, cancellationToken);
                await _reviewRepository.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review");
            }
        }
    }
}
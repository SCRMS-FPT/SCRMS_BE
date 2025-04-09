using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.AdminDeleteReview
{
    public record AdminDeleteReviewCommand(Guid ReviewId, Guid AdminId) : IRequest;

    public class AdminDeleteReviewHandler : IRequestHandler<AdminDeleteReviewCommand>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<AdminDeleteReviewHandler> _logger;

        public AdminDeleteReviewHandler(IReviewRepository reviewRepository, ILogger<AdminDeleteReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task Handle(AdminDeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(request.ReviewId, cancellationToken);
            if (review == null)
                throw new ArgumentException("Review not found");

            // Ghi log hành động xóa bởi admin
            _logger.LogInformation("Admin {AdminId} deleted review {ReviewId} created by user {ReviewerId}",
                request.AdminId, request.ReviewId, review.ReviewerId);

            await _reviewRepository.RemoveReviewAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
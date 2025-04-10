using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.UpdateReviewFlagStatus
{
    public record UpdateReviewFlagStatusCommand(
        Guid FlagId,
        string Status,
        string? AdminNote) : IRequest;

    public class UpdateReviewFlagStatusHandler : IRequestHandler<UpdateReviewFlagStatusCommand>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<UpdateReviewFlagStatusHandler> _logger;

        public UpdateReviewFlagStatusHandler(
            IReviewRepository reviewRepository,
            ILogger<UpdateReviewFlagStatusHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task Handle(UpdateReviewFlagStatusCommand request, CancellationToken cancellationToken)
        {
            var flag = await _reviewRepository.GetReviewFlagByIdAsync(request.FlagId, cancellationToken);

            if (flag == null)
                throw new ArgumentException($"Flag with ID {request.FlagId} not found");

            // Cập nhật trạng thái và thông tin khác
            flag.Status = request.Status;
            flag.UpdatedAt = DateTime.UtcNow;

            // Lưu thay đổi
            await _reviewRepository.UpdateReviewFlagAsync(flag, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Flag {FlagId} for review {ReviewId} has been {Status} by admin",
                flag.Id, flag.ReviewId, request.Status);
        }
    }
}
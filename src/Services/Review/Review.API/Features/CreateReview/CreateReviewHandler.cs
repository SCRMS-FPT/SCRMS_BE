using Reviews.API.Cache;
using Reviews.API.Clients;
using Reviews.API.Data.Repositories;

namespace Reviews.API.Features.CreateReview
{
    public record CreateReviewCommand(Guid ReviewerId, string SubjectType, Guid SubjectId, int Rating, string? Comment) : IRequest<Guid>;

    public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Guid>
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ISubjectCache _cache;
        private readonly ICoachServiceClient _coachClient;
        private readonly ICourtServiceClient _courtClient;
        private readonly ILogger<CreateReviewHandler> _logger;

        public CreateReviewHandler(
            IReviewRepository reviewRepository,
            ISubjectCache cache,
            ICoachServiceClient coachClient,
            ICourtServiceClient courtClient,
            ILogger<CreateReviewHandler> logger)
        {
            _reviewRepository = reviewRepository;
            _cache = cache;
            _coachClient = coachClient;
            _courtClient = courtClient;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            if (request.Rating < 1 || request.Rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");
            if (!new[] { "court", "coach" }.Contains(request.SubjectType))
                throw new ArgumentException("Invalid subject type.");
            // var cacheKey = $"{request.SubjectType}_{request.SubjectId}";
            // if (!_cache.TryGetValue(cacheKey, out var exists))
            // {
            //     exists = await CheckViaApi(request.SubjectType, request.SubjectId, cancellationToken);
            //     _cache.Set(cacheKey, exists);
            // }

            // if (!exists)
            //     throw new InvalidOperationException($"{request.SubjectType} not exists");
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

            await _reviewRepository.AddReviewAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);
            return review.Id;
        }

        private async Task<bool> CheckViaApi(string subjectType, Guid subjectId, CancellationToken ct)
        {
            try
            {
                return subjectType switch
                {
                    "coach" => await _coachClient.CoachExistsAsync(subjectId, ct),
                    "court" => await _courtClient.CourtExistsAsync(subjectId, ct),
                    _ => throw new ArgumentException("Invalid subject type")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking {SubjectType} existence", subjectType);
                throw new ServiceUnavailableException("Validation service unavailable");
            }
        }

        public class ServiceUnavailableException : Exception
        {
            public ServiceUnavailableException()
            { }

            public ServiceUnavailableException(string message) : base(message)
            {
            }

            public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
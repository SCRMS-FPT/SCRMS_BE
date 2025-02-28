using Microsoft.EntityFrameworkCore;

namespace Reviews.API.Features.GetReviewsByCoachId
{
    public record GetReviewsByCoachIdQuery(Guid coachId, int Page, int Limit) : IRequest<List<ReviewResponse>>;

    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt);
    public class GetReviewsByCoachIdQueryValidator : AbstractValidator<GetReviewsByCoachIdQuery>
    {
        public GetReviewsByCoachIdQueryValidator()
        {
            RuleFor(x => x.coachId)
                .NotEmpty().WithMessage("CoachId is required.");
        }
    }
    public class GetReviewsByCoachIdHandler : IRequestHandler<GetReviewsByCoachIdQuery, List<ReviewResponse>>
    {
        private readonly ReviewDbContext _context;

        public GetReviewsByCoachIdHandler(ReviewDbContext context) => _context = context;

        public async Task<List<ReviewResponse>> Handle(GetReviewsByCoachIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.Reviews
                .Where(r => r.SubjectId == request.coachId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.Page - 1) * request.Limit)
                .Take(request.Limit)
                .Select(r => new ReviewResponse(r.Id, r.ReviewerId, r.Rating, r.Comment, r.CreatedAt))
                .ToListAsync(cancellationToken);
        }
    }
}

using BuildingBlocks.Pagination;
using Microsoft.EntityFrameworkCore;
using Reviews.API.Data.Repositories;
using Reviews.API.Features.GetReviewReplies;

namespace Reviews.API.Features.GetReviewsByCoachId
{
    public record GetReviewsByCoachIdQuery(Guid CoachId, int Page, int Limit) : IRequest<PaginatedResult<ReviewResponse>>;
    public record ReviewResponse(Guid Id, Guid ReviewerId, int Rating, string? Comment, DateTime CreatedAt, List<ReviewReplyResponse> ListReplies);

    public class GetReviewsByCoachIdQueryValidator : AbstractValidator<GetReviewsByCoachIdQuery>
    {
        public GetReviewsByCoachIdQueryValidator()
        {
            RuleFor(x => x.CoachId)
                .NotEmpty().WithMessage("CoachId is required.");
        }
    }

    public class GetReviewsByCoachIdHandler : IRequestHandler<GetReviewsByCoachIdQuery, PaginatedResult<ReviewResponse>>
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewsByCoachIdHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<PaginatedResult<ReviewResponse>> Handle(GetReviewsByCoachIdQuery request, CancellationToken cancellationToken)
        {
            // Lấy tổng số review của coach
            var totalCount = await _reviewRepository.CountReviewsByCoachIdAsync(request.CoachId, cancellationToken);

            // Lấy danh sách review phân trang
            var reviews = await _reviewRepository.GetReviewsByCoachIdAsync(request.CoachId, request.Page, request.Limit, cancellationToken);

            // Chuyển đổi sang ReviewResponse
            var reviewResponses = reviews.Select(r => new ReviewResponse(
                r.Id,
                r.ReviewerId,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                r.Replies.Select(reply => new ReviewReplyResponse(reply.Id, reply.ResponderId, reply.ReplyText, reply.CreatedAt)).ToList()
            )).ToList();

            // Trả về kết quả phân trang
            return new PaginatedResult<ReviewResponse>
            (request.Page,
                request.Limit,
                 totalCount,
                 reviewResponses
            );
        }
    }
}
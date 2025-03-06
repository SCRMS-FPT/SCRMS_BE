using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Suggestions
{
    public record GetSuggestionsQuery(int Page, int Limit, Guid UserId) : IRequest<List<UserProfile>>;

    public class GetSuggestionsHandler : IRequestHandler<GetSuggestionsQuery, List<UserProfile>>
    {
        private readonly IUserSkillRepository _userSkillRepository;

        public GetSuggestionsHandler(IUserSkillRepository userSkillRepository)
        {
            _userSkillRepository = userSkillRepository;
        }

        public async Task<List<UserProfile>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
        {
            // Gọi repository để lấy danh sách ID gợi ý
            var suggestionUserIds = await _userSkillRepository.GetSuggestionUserIdsAsync(
                request.UserId,
                request.Page,
                request.Limit,
                cancellationToken
            );

            // Chuyển đổi danh sách ID thành danh sách UserProfile
            return suggestionUserIds.Select(id => new UserProfile { Id = id }).ToList();
        }
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
    }
}
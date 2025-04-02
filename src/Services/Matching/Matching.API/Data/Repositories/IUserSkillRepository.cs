using System.Threading.Tasks;
using Matching.API.Features.Suggestions;

namespace Matching.API.Data.Repositories
{
    public interface IUserSkillRepository
    {
        Task<UserSkill?> GetByUserIdAndSportIdAsync(Guid userId, Guid sportId, CancellationToken cancellationToken);

        Task<List<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

        Task AddUserSkillAsync(UserSkill userSkill, CancellationToken cancellationToken);

        Task UpdateUserSkillAsync(UserSkill userSkill, CancellationToken cancellationToken);

        Task<List<Guid>> GetSuggestionUserIdsAsync(Guid userId, int page, int limit, CancellationToken cancellationToken);

        Task<Dictionary<Guid, List<UserSkill>>> GetSuggestionsWithSkillsAsync(
            Guid userId,
            int page,
            int limit,
            List<SportSkillFilter> sportSkillFilters,
            CancellationToken cancellationToken);
    }
}
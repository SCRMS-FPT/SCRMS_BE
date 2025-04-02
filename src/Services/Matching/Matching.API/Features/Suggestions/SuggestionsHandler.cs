using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Suggestions
{
    public record GetSuggestionsQuery(
        int Page,
        int Limit,
        Guid UserId,
        List<SportSkillFilter> SportSkillFilters) : IRequest<List<UserProfile>>;

    public class GetSuggestionsHandler : IRequestHandler<GetSuggestionsQuery, List<UserProfile>>
    {
        private readonly IUserSkillRepository _userSkillRepository;

        public GetSuggestionsHandler(IUserSkillRepository userSkillRepository)
        {
            _userSkillRepository = userSkillRepository;
        }

        public async Task<List<UserProfile>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
        {
            var userSkillsMap = await _userSkillRepository.GetSuggestionsWithSkillsAsync(
                request.UserId,
                request.Page,
                request.Limit,
                request.SportSkillFilters,
                cancellationToken
            );

            var profiles = new List<UserProfile>();

            foreach (var userSkills in userSkillsMap)
            {
                var userId = userSkills.Key;
                var skillsList = userSkills.Value;

                var profile = new UserProfile
                {
                    Id = userId,
                    Sports = skillsList.Select(skill => new UserSportSkill
                    {
                        SportId = skill.SportId,
                        SkillLevel = skill.SkillLevel
                    }).ToList()
                };

                profiles.Add(profile);
            }

            return profiles;
        }
    }

    public class UserProfile
    {
        public Guid Id { get; set; }
        public List<UserSportSkill> Sports { get; set; } = new List<UserSportSkill>();
    }

    public class UserSportSkill
    {
        public Guid SportId { get; set; }
        public string SkillLevel { get; set; }
    }
}
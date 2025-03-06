using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Skills.GetUserSkills
{
    public record GetUserSkillsQuery(Guid UserId) : IRequest<List<UserSkillResponse>>;

    public record UserSkillResponse(Guid SportId, string SkillLevel);

    public class GetUserSkillsHandler : IRequestHandler<GetUserSkillsQuery, List<UserSkillResponse>>
    {
        private readonly IUserSkillRepository _userSkillRepository;

        public GetUserSkillsHandler(IUserSkillRepository userSkillRepository)
        {
            _userSkillRepository = userSkillRepository;
        }

        public async Task<List<UserSkillResponse>> Handle(GetUserSkillsQuery request, CancellationToken cancellationToken)
        {
            var skills = await _userSkillRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            return skills.Select(us => new UserSkillResponse(us.SportId, us.SkillLevel)).ToList();
        }
    }
}
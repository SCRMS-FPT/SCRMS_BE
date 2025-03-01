using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Skills.GetUserSkills
{
    public record GetUserSkillsQuery(Guid UserId) : IRequest<List<UserSkillResponse>>;

    public record UserSkillResponse(Guid SportId, string SkillLevel);

    public class GetUserSkillsHandler : IRequestHandler<GetUserSkillsQuery, List<UserSkillResponse>>
    {
        private readonly MatchingDbContext _context;

        public GetUserSkillsHandler(MatchingDbContext context) => _context = context;

        public async Task<List<UserSkillResponse>> Handle(GetUserSkillsQuery request, CancellationToken cancellationToken)
        {
            return await _context.UserSkills
                .Where(us => us.UserId == request.UserId)
                .Select(us => new UserSkillResponse(us.SportId, us.SkillLevel))
                .ToListAsync(cancellationToken);
        }
    }
}
using Matching.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Skills.UpdateUserSkill
{
    public record UpdateUserSkillCommand(Guid UserId, Guid SportId, string SkillLevel) : IRequest;

    public class UpdateUserSkillHandler : IRequestHandler<UpdateUserSkillCommand>
    {
        private readonly MatchingDbContext _context;

        public UpdateUserSkillHandler(MatchingDbContext context) => _context = context;

        public async Task Handle(UpdateUserSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == request.UserId && us.SportId == request.SportId, cancellationToken);

            if (skill == null)
            {
                skill = new UserSkill
                {
                    UserId = request.UserId,
                    SportId = request.SportId,
                    SkillLevel = request.SkillLevel
                };
                _context.UserSkills.Add(skill);
            }
            else
            {
                skill.SkillLevel = request.SkillLevel;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
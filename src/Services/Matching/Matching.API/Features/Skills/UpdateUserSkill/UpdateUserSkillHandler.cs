using Matching.API.Data;
using Matching.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Matching.API.Features.Skills.UpdateUserSkill
{
    public record UpdateUserSkillCommand(Guid UserId, Guid SportId, string SkillLevel) : IRequest;

    public class UpdateUserSkillHandler : IRequestHandler<UpdateUserSkillCommand>
    {
        private readonly IUserSkillRepository _userSkillRepository;
        private readonly MatchingDbContext _context;

        public UpdateUserSkillHandler(IUserSkillRepository userSkillRepository, MatchingDbContext context)
        {
            _userSkillRepository = userSkillRepository;
            _context = context;
        }

        public async Task Handle(UpdateUserSkillCommand request, CancellationToken cancellationToken)
        {
            var skill = await _userSkillRepository.GetByUserIdAndSportIdAsync(request.UserId, request.SportId, cancellationToken);
            if (skill == null)
            {
                skill = new UserSkill
                {
                    UserId = request.UserId,
                    SportId = request.SportId,
                    SkillLevel = request.SkillLevel
                };
                await _userSkillRepository.AddUserSkillAsync(skill, cancellationToken);
            }
            else
            {
                skill.SkillLevel = request.SkillLevel;
                await _userSkillRepository.UpdateUserSkillAsync(skill, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
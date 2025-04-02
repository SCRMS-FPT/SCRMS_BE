using Matching.API.Data;
using Matching.API.Data.Models;
using Matching.API.Data.Repositories;

namespace Matching.API.Features.Skills.CreateUserSkill
{
    public record CreateUserSkillCommand(Guid UserId, Guid SportId, string SkillLevel) : IRequest;

    public class CreateUserSkillValidator : AbstractValidator<CreateUserSkillCommand>
    {
        public CreateUserSkillValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
            RuleFor(x => x.SportId).NotEmpty().WithMessage("Sport ID is required");
            RuleFor(x => x.SkillLevel).NotEmpty().WithMessage("Skill level is required")
                .Must(level => level == "Beginner" || level == "Intermediate" || level == "Advanced")
                .WithMessage("Skill level must be either 'Beginner', 'Intermediate', or 'Advanced'");
        }
    }

    public class CreateUserSkillHandler : IRequestHandler<CreateUserSkillCommand>
    {
        private readonly IUserSkillRepository _userSkillRepository;
        private readonly MatchingDbContext _context;

        public CreateUserSkillHandler(IUserSkillRepository userSkillRepository, MatchingDbContext context)
        {
            _userSkillRepository = userSkillRepository;
            _context = context;
        }

        public async Task Handle(CreateUserSkillCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra kỹ năng đã tồn tại chưa
            var existingSkill = await _userSkillRepository.GetByUserIdAndSportIdAsync(
                request.UserId, request.SportId, cancellationToken);

            if (existingSkill != null)
            {
                throw new InvalidOperationException($"Skill for sport {request.SportId} already exists for this user. Use the update endpoint instead.");
            }

            // Tạo kỹ năng mới
            var newSkill = new UserSkill
            {
                UserId = request.UserId,
                SportId = request.SportId,
                SkillLevel = request.SkillLevel,
            };

            // Thêm vào database
            await _userSkillRepository.AddUserSkillAsync(newSkill, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
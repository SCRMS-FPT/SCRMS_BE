using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Coaches.CreateCoach
{
    public record CreateCoachCommand(
        Guid UserId,
        string Bio,
        decimal RatePerHour,
        List<Guid> SportIds
    ) : ICommand<CreateCoachResult>;
    public record CreateCoachResult(Guid Id);

    public class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>
    {
        public CreateCoachCommandValidator()
        {
            RuleFor(x => x.Bio).NotEmpty();
            RuleFor(x => x.RatePerHour).GreaterThan(0);
            RuleFor(x => x.SportIds).NotEmpty().WithMessage("At least one sport required");
        }
    }

    internal class CreateCoachCommandHandler : ICommandHandler<CreateCoachCommand, CreateCoachResult>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachSportRepository _sportRepository;
        private readonly CoachDbContext _context;

        public CreateCoachCommandHandler(
            ICoachRepository coachRepository,
            ICoachSportRepository sportRepository,
            CoachDbContext context)
        {
            _coachRepository = coachRepository;
            _sportRepository = sportRepository;
            _context = context;
        }

        public async Task<CreateCoachResult> Handle(
            CreateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var exists = await _coachRepository.CoachExistsAsync(command.UserId, cancellationToken);
            if (exists)
                throw new AlreadyExistsException("Coach", command.UserId);

            var coach = new Models.Coach
            {
                UserId = command.UserId,
                Bio = command.Bio,
                RatePerHour = command.RatePerHour,
                CreatedAt = DateTime.UtcNow
            };

            var coachSports = command.SportIds.Select(sportId => new CoachSport
            {
                CoachId = coach.UserId,
                SportId = sportId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _coachRepository.AddCoachAsync(coach, cancellationToken);
            foreach (var sport in coachSports)
            {
                await _sportRepository.AddCoachSportAsync(sport, cancellationToken);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateCoachResult(coach.UserId);
        }
    }
}
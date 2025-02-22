using Coach.API.Data;

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
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Bio).NotEmpty();
            RuleFor(x => x.RatePerHour).GreaterThan(0);
        }
    }

    internal class CreateCoachCommandHandler(CoachDbContext context)
    : ICommandHandler<CreateCoachCommand, CreateCoachResult>
    {
        public async Task<CreateCoachResult> Handle(
            CreateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var coach = new Models.Coach
            {
                UserId = command.UserId,
                Bio = command.Bio,
                RatePerHour = command.RatePerHour,
                CreatedAt = DateTime.UtcNow
            };

            // Thêm các môn thể thao
            var coachSports = command.SportIds.Select(sportId => new CoachSport
            {
                CoachId = coach.UserId,
                SportId = sportId,
                CreatedAt = DateTime.UtcNow
            });

            await context.Coaches.AddAsync(coach, cancellationToken);
            await context.CoachSports.AddRangeAsync(coachSports, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new CreateCoachResult(coach.UserId);
        }
    }
}
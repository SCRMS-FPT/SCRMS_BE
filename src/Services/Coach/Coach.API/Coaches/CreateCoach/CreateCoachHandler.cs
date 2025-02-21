using Coach.API.Data;

namespace Coach.API.Coaches.CreateCoach
{
    public record CreateCoachCommand(Guid UserId, int SportId, string Bio, decimal RatePerHour) : ICommand<CreateCoachResult>;

    public record CreateCoachResult(Guid Id);

    public class CreateCoachCommandValidator : AbstractValidator<CreateCoachCommand>

    {
        public CreateCoachCommandValidator()

        {
            RuleFor(x => x.UserId).NotEmpty();

            RuleFor(x => x.SportId).GreaterThan(0);

            RuleFor(x => x.Bio).NotEmpty();

            RuleFor(x => x.RatePerHour).GreaterThan(0);
        }
    }

    internal class CreateCoachCommandHandler(CoachDbContext context)
    : ICommandHandler<CreateCoachCommand, CreateCoachResult>
    {
        public async Task<CreateCoachResult> Handle(CreateCoachCommand command, CancellationToken cancellationToken)
        {
            var coach = new Models.Coach
            {
                UserId = command.UserId,
                SportId = command.SportId,
                Bio = command.Bio,
                RatePerHour = command.RatePerHour,
                CreatedAt = DateTime.UtcNow
            };

            context.Coaches.Add(coach);
            await context.SaveChangesAsync(cancellationToken);

            return new CreateCoachResult(coach.UserId);
        }
    }
}
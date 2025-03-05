using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Coaches.UpdateCoach
{
    public record UpdateCoachCommand(Guid CoachId, string Bio, decimal RatePerHour, List<Guid> listSport) : ICommand<Unit>;
    public class UpdateCoachCommandValidator : AbstractValidator<UpdateCoachCommand>
    {
        public UpdateCoachCommandValidator()
        {
            RuleFor(x => x.Bio).NotEmpty().WithMessage("Bio is required");
            RuleFor(x => x.CoachId).NotEmpty().WithMessage("Coach id is required");
            RuleFor(x => x.RatePerHour).GreaterThan(0).WithMessage("Rate per hour must greater than 0");
        }
    }
    public class UpdateCoachCommandHandler(CoachDbContext context)
    : ICommandHandler<UpdateCoachCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var coach = await context.Coaches.Include(p => p.CoachSports).FirstOrDefaultAsync(c => c.UserId == command.CoachId);
            if (coach == null)
            {
                throw new CoachNotFoundException(command.CoachId);
            }
            coach.Bio = command.Bio;
            coach.RatePerHour = command.RatePerHour;

            // REMOVE EVERY SPORT OF COACH
            var listOldSport = coach.CoachSports.ToList();
            context.CoachSports.RemoveRange(listOldSport);
            await context.SaveChangesAsync();
            // ADD NEW LIST OF SPORT
            var listNewSport = command.listSport.Select(p => new CoachSport
            {
                CoachId = coach.UserId,
                SportId = p,
                CreatedAt = DateTime.UtcNow
            }).ToList();
            await context.CoachSports.AddRangeAsync(listNewSport);
            context.Coaches.Update(coach);
            await context.SaveChangesAsync();
            return Unit.Value;
        }
    }
}

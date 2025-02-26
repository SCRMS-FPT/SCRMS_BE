using Coach.API.Data;
using Coach.API.Schedules.UpdateSchedule;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication.PgOutput.Messages;

namespace Coach.API.Dashboard.GetStat
{
    public record GetStatsCommand(
        DateOnly StartTime,
        DateOnly EndTime
    ) : ICommand<GetStatsResult>;

    public record GetStatsResult(decimal Revenue, decimal TotalRate, int NumberOfLessons);
    public class GetStatsCommandValidator : AbstractValidator<GetStatsCommand>
    {
        public GetStatsCommandValidator()
        {
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("End time must in the future of start time");
        }
    }
    public class GetStatsCommandHandler : ICommandHandler<GetStatsCommand, GetStatsResult>
    {
        private readonly CoachDbContext context;

        public GetStatsCommandHandler(CoachDbContext context)
        {
            this.context = context;
        }

        public async Task<GetStatsResult> Handle(GetStatsCommand command, CancellationToken cancellationToken)
        {
            List<CoachBooking> bookings = context.CoachBookings
                .Where(cb => (cb.BookingDate.CompareTo(command.StartTime) >= 0 && cb.BookingDate.CompareTo(command.EndTime) <= 0))
                .ToList();

            decimal totalRate = 0;
            decimal totalRevenues = 0;
            int numberOfLessons = 0;
            foreach (var booking in bookings)
            {
                var package = await context.CoachPackages.FirstOrDefaultAsync(cb => cb.Id == booking.PackageId);
                var coach = await context.Coaches.FirstOrDefaultAsync(c => c.UserId == booking.CoachId);
                if (package != null)
                {
                    totalRevenues += booking.TotalPrice;
                    numberOfLessons += package.SessionCount;
                }
                if (coach != null)
                {
                    // TODO: This really confuse because of RatePerHour attribute, which must change in the future
                    totalRate += coach.RatePerHour;
                }
            }

            return new GetStatsResult(totalRevenues, totalRate / bookings.Count(), numberOfLessons);
        }

    }
}

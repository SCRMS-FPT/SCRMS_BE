using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Dashboard.GetStat
{
    // Update to use nullable DateOnly?
    public record GetStatsCommand(
        DateOnly? StartTime,  // Make StartTime nullable
        DateOnly? EndTime     // Make EndTime nullable
    ) : ICommand<GetStatsResult>;

    public record GetStatsResult(decimal Revenue, decimal TotalRate, int NumberOfLessons);

    public class GetStatsCommandValidator : AbstractValidator<GetStatsCommand>
    {
        public GetStatsCommandValidator()
        {
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("End time must be in the future of start time");
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
            IQueryable<CoachBooking> bookingsQuery = context.CoachBookings;

            // Apply date filters if they are provided
            if (command.StartTime.HasValue && command.EndTime.HasValue)
            {
                bookingsQuery = bookingsQuery
                    .Where(cb => cb.BookingDate.CompareTo(command.StartTime.Value) >= 0
                                 && cb.BookingDate.CompareTo(command.EndTime.Value) <= 0);
            }
            else if (command.StartTime.HasValue)
            {
                bookingsQuery = bookingsQuery
                    .Where(cb => cb.BookingDate.CompareTo(command.StartTime.Value) >= 0);
            }
            else if (command.EndTime.HasValue)
            {
                bookingsQuery = bookingsQuery
                    .Where(cb => cb.BookingDate.CompareTo(command.EndTime.Value) <= 0);
            }

            List<CoachBooking> bookings = await bookingsQuery.ToListAsync(cancellationToken);

            decimal totalRate = 0;
            decimal totalRevenues = 0;
            int numberOfLessons = 0;

            foreach (var booking in bookings)
            {
                var package = await context.CoachPackages.FirstOrDefaultAsync(cb => cb.Id == booking.PackageId);
                var coach = await context.Coaches.FirstOrDefaultAsync(c => c.UserId == booking.CoachId);

                if (package != null)
                {
                    // TODO: Must check again
                    totalRevenues += booking.TotalPrice;
                    numberOfLessons += package.SessionCount;
                }
                else
                {
                    if (coach != null)
                    {
                        totalRevenues += coach.RatePerHour;
                    }
                }
            }

            if (bookings.Count == 0)
            {
                return new GetStatsResult(0, 0, 0);  // No bookings found, return zero values
            }

            // Return the result, ensuring no division by zero
            return new GetStatsResult(totalRevenues, totalRate / bookings.Count(), numberOfLessons);
        }
    }
}
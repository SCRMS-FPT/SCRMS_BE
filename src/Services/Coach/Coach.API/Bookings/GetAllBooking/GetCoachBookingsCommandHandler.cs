using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.GetAllBooking
{
    public record GetCoachBookingsQuery(Guid CoachUserId) : IQuery<List<BookingHistoryResult>>;

    public record BookingHistoryResult(
        Guid Id,
        Guid UserId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        decimal TotalPrice
    );

    internal class GetCoachBookingsCommandHandler
        : IQueryHandler<GetCoachBookingsQuery, List<BookingHistoryResult>>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public GetCoachBookingsCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<List<BookingHistoryResult>> Handle(GetCoachBookingsQuery query, CancellationToken cancellationToken)
        {
            return await context.CoachBookings
                .Where(b => b.CoachId == query.CoachUserId)
                .Select(b => new BookingHistoryResult(
                    b.Id, b.UserId, b.BookingDate, b.StartTime, b.EndTime, b.Status, b.TotalPrice))
                .ToListAsync(cancellationToken);
        }
    }
}

using Coach.API.Bookings.CreateBooking;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.GetAllBooking
{
    public record GetCoachBookingsQuery(Guid CoachUserId, int Page, int RecordPerPage, string? Status, DateOnly? StartDate, DateOnly? EndDate) : IQuery<List<BookingHistoryResult>>;

    public record BookingHistoryResult(
        Guid Id,
        Guid UserId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        decimal TotalPrice
    );
    public class GetCoachBookingsQueryValidator : AbstractValidator<GetCoachBookingsQuery>
    {
        public GetCoachBookingsQueryValidator()
        {
            RuleFor(x => x.CoachUserId).NotEmpty().WithMessage("CoachId is required.");
        }
    }
    internal class GetCoachBookingsQueryHandler
        : IQueryHandler<GetCoachBookingsQuery, List<BookingHistoryResult>>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public GetCoachBookingsQueryHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<List<BookingHistoryResult>> Handle(GetCoachBookingsQuery query, CancellationToken cancellationToken)
        {
            var list = await context.CoachBookings.ToListAsync(cancellationToken); 
            if (query.Status != null)
            {
                list = list.Where(l => l.Status.Equals(query.Status)).ToList();
            }
            if (query.EndDate != null && query.StartDate != null)
            {
                list =list.Where(l => l.BookingDate.CompareTo(query.StartDate) >= 0 && l.BookingDate.CompareTo(query.EndDate) <= 0).ToList();
            }
            
            return list
                .Where(b => b.CoachId == query.CoachUserId)
                .Select(b => new BookingHistoryResult(
                    b.Id, b.UserId, b.BookingDate, b.StartTime, b.EndTime, b.Status, b.TotalPrice))
                .Skip((query.Page - 1 ) * query.RecordPerPage)
                .Take(query.RecordPerPage)
                .ToList();
        }
    }
}

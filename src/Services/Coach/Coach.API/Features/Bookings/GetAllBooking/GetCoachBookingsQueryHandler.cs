using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.GetAllBooking
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

    internal class GetCoachBookingsQueryHandler : IQueryHandler<GetCoachBookingsQuery, List<BookingHistoryResult>>
    {
        private readonly ICoachBookingRepository _bookingRepository;

        public GetCoachBookingsQueryHandler(ICoachBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<List<BookingHistoryResult>> Handle(GetCoachBookingsQuery query, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetCoachBookingsByCoachIdAsync(query.CoachUserId, cancellationToken);
            if (query.Status != null)
            {
                bookings = bookings.Where(b => b.Status == query.Status).ToList();
            }
            if (query.StartDate != null && query.EndDate != null)
            {
                bookings = bookings.Where(b => b.BookingDate >= query.StartDate && b.BookingDate <= query.EndDate).ToList();
            }

            return bookings
                .Select(b => new BookingHistoryResult(
                    b.Id, b.UserId, b.BookingDate, b.StartTime, b.EndTime, b.Status, b.TotalPrice))
                .Skip((query.Page - 1) * query.RecordPerPage)
                .Take(query.RecordPerPage)
                .ToList();
        }
    }
}
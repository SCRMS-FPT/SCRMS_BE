using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.GetBookingById
{
    public record GetBookingByIdQuery(Guid BookingId) : IQuery<BookingDetailResult>;

    public record BookingDetailResult(
        Guid Id,
        Guid UserId,
        Guid CoachId,
        Guid SportId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        string Status,
        decimal TotalPrice,
        Guid? PackageId
    );

    public class GetBookingByIdCommandValidator : AbstractValidator<GetBookingByIdQuery>
    {
        public GetBookingByIdCommandValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty().WithMessage("BookingId is required.");
        }
    }

    public class GetBookingByIdQueryHandler : IQueryHandler<GetBookingByIdQuery, BookingDetailResult>
    {
        private readonly ICoachBookingRepository _bookingRepository;

        public GetBookingByIdQueryHandler(ICoachBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<BookingDetailResult> Handle(GetBookingByIdQuery query, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetCoachBookingByIdAsync(query.BookingId, cancellationToken);
            if (booking == null)
                throw new NotFoundException("Booking not found");

            return new BookingDetailResult(
                booking.Id, booking.UserId, booking.CoachId, booking.SportId, booking.BookingDate,
                booking.StartTime, booking.EndTime, booking.Status, booking.TotalPrice, booking.PackageId);
        }
    }
}
using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.GetBookingById
{
    public record GetBookingByIdRequest(Guid BookingId) : IQuery<BookingDetailResult>;

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

    public class GetBookingByIdCommandValidator : AbstractValidator<GetBookingByIdRequest>
    {
        public GetBookingByIdCommandValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty().WithMessage("BookingId is required.");
        }
    }

    internal class GetBookingByIdCommandHandler
       : IQueryHandler<GetBookingByIdRequest, BookingDetailResult>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public GetBookingByIdCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<BookingDetailResult> Handle(GetBookingByIdRequest query, CancellationToken cancellationToken)
        {
            var booking = await context.CoachBookings
                .Where(b => b.Id == query.BookingId)
                .Select(b => new BookingDetailResult(
                    b.Id, b.UserId, b.CoachId, b.SportId, b.BookingDate,
                    b.StartTime, b.EndTime, b.Status, b.TotalPrice, b.PackageId))
                .FirstOrDefaultAsync(cancellationToken);

            return booking ?? throw new NotFoundException("Booking not found");
        }
    }
}

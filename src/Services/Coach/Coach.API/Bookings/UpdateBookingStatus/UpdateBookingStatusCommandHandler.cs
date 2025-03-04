using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Schedules.UpdateSchedule;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.UpdateBooking
{
    public record UpdateBookingStatusCommand(Guid BookingId, string Status, Guid CoachBookingId) : ICommand<UpdateBookingStatusResult>;

    public record UpdateBookingStatusResult(Boolean IsUpdated);

    public class UpdateBookingStatusCommandValidator : AbstractValidator<UpdateBookingStatusCommand>
    {
        public UpdateBookingStatusCommandValidator()
        {
            RuleFor(x => x.BookingId).NotEmpty()
                .WithMessage("BookingId is required.");
            RuleFor(x => x.Status)
            .NotEmpty()
            .Must(status => status == "confirmed" || status == "cancelled")
            .WithMessage("Status must be either 'confirmed' or 'cancelled'.");
        }
    }

    internal class UpdateBookingStatusCommandHandler
       : ICommandHandler<UpdateBookingStatusCommand, UpdateBookingStatusResult>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public UpdateBookingStatusCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<UpdateBookingStatusResult> Handle(UpdateBookingStatusCommand command, CancellationToken cancellationToken)
        {
            var booking = await context.CoachBookings
                .FirstOrDefaultAsync(b => b.Id == command.BookingId, cancellationToken);

            if (booking == null)
                throw new NotFoundException("Booking not found");

            if (booking.CoachId != command.CoachBookingId)
            {
                throw new ValidationException("Booking coach is not you");
            }

            if (command.Status != "confirmed" && command.Status != "cancelled")
                throw new ValidationException("Invalid booking status");

            booking.Status = command.Status;
            await context.SaveChangesAsync(cancellationToken);

            return new UpdateBookingStatusResult(true);
        }
    }
}
using BuildingBlocks.Exceptions;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Features.Schedules.UpdateSchedule;
using global::Coach.API.Data.Repositories;
using global::Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.UpdateBookingStatus
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

    public class UpdateBookingStatusCommandHandler : ICommandHandler<UpdateBookingStatusCommand, UpdateBookingStatusResult>
    {
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly CoachDbContext _context;

        public UpdateBookingStatusCommandHandler(ICoachBookingRepository bookingRepository, CoachDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<UpdateBookingStatusResult> Handle(UpdateBookingStatusCommand command, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetCoachBookingByIdAsync(command.BookingId, cancellationToken);
            if (booking == null)
                throw new NotFoundException("Booking not found");

            if (booking.CoachId != command.CoachBookingId)
                throw new ValidationException("Booking coach is not you");

            if (command.Status != "confirmed" && command.Status != "cancelled")
                throw new ValidationException("Invalid booking status");

            booking.Status = command.Status;
            await _bookingRepository.UpdateCoachBookingAsync(booking, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateBookingStatusResult(true);
        }
    }
}
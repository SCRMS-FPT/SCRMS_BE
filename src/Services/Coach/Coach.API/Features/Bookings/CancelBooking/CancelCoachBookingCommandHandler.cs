using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events;
using BuildingBlocks.Messaging.Outbox;
using Coach.API.Data;
using Coach.API.Data.Repositories;
using FluentValidation;

namespace Coach.API.Features.Bookings.CancelBooking
{
    public record CancelCoachBookingCommand(
        Guid BookingId,
        string CancellationReason,
        DateTime RequestedAt,
        Guid UserId,
        string Role
    ) : ICommand<CancelCoachBookingResult>;

    public record CancelCoachBookingResult(
        Guid BookingId,
        string Status,
        decimal RefundAmount,
        string Message
    );

    public class CancelCoachBookingCommandValidator : AbstractValidator<CancelCoachBookingCommand>
    {
        public CancelCoachBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
            RuleFor(c => c.CancellationReason).NotEmpty().MaximumLength(500);
            RuleFor(c => c.RequestedAt).NotEmpty();
            RuleFor(c => c.UserId).NotEmpty();
        }
    }

    public class CancelCoachBookingCommandHandler : ICommandHandler<CancelCoachBookingCommand, CancelCoachBookingResult>
    {
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly ICoachRepository _coachRepository;
        private readonly IOutboxService _outboxService;
        private readonly CoachDbContext _dbContext;
        private readonly ILogger<CancelCoachBookingCommandHandler> _logger;

        public CancelCoachBookingCommandHandler(
            ICoachBookingRepository bookingRepository,
            ICoachRepository coachRepository,
            IOutboxService outboxService,
            CoachDbContext dbContext,
            ILogger<CancelCoachBookingCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _coachRepository = coachRepository;
            _outboxService = outboxService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<CancelCoachBookingResult> Handle(CancelCoachBookingCommand request, CancellationToken cancellationToken)
        {
            // Get the booking by ID
            var booking = await _bookingRepository.GetCoachBookingByIdAsync(request.BookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException($"Booking with ID {request.BookingId} not found");
            }

            // Check if the booking is already cancelled or completed
            if (booking.Status == "cancelled")
            {
                throw new InvalidOperationException("The booking is already cancelled");
            }
            // Check if the user is authorized to cancel this booking
            bool isAuthorized = false;

            // User is the booking owner
            if (booking.UserId == request.UserId)
            {
                isAuthorized = true;
            }

            // User is the coach
            if (booking.CoachId == request.UserId)
            {
                isAuthorized = true;
            }

            // User is an admin
            if (request.Role == "Admin")
            {
                isAuthorized = true;
            }

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("You don't have permission to cancel this booking");
            }

            // Begin transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Calculate refund amount (if applicable)
                decimal refundAmount = booking.TotalPrice;

                // Update booking status and cancellation reason
                booking.Status = "cancelled";

                // Save changes to the database
                await _bookingRepository.UpdateCoachBookingAsync(booking, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Save the integration event to the outbox
                var coachBookingCancelledRefundEvent = new CoachBookingCancelledRefundEvent(
                    booking.Id,
                    booking.UserId,
                    booking.CoachId,
                    refundAmount,
                    request.CancellationReason,
                    request.RequestedAt);

                await _outboxService.SaveMessageAsync(coachBookingCancelledRefundEvent);

                // Also save notification event for other systems
                var coachBookingCancelledNotificationEvent = new CoachBookingCancelledNotificationEvent(
                    booking.Id,
                    booking.UserId,
                    booking.CoachId,
                    refundAmount > 0,
                    refundAmount,
                    request.CancellationReason,
                    request.RequestedAt);

                await _outboxService.SaveMessageAsync(coachBookingCancelledNotificationEvent);

                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);

                // Return result
                return new CancelCoachBookingResult(
                    booking.Id,
                    "cancelled",
                    refundAmount,
                    "Booking cancelled successfully"
                );
            }
            catch (Exception ex)
            {
                // Rollback the transaction if an error occurred
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error cancelling coach booking {BookingId}", request.BookingId);
                throw;
            }
        }
    }
}
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Messaging.Events;
using CourtBooking.Application.Data;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.Events;
using CourtBooking.Domain.ValueObjects;
using MediatR;

namespace CourtBooking.Application.BookingManagement.Command.CancelBookingByOwner
{
    public class CancelBookingByOwnerCommandHandler : IRequestHandler<CancelBookingByOwnerCommand, CancelBookingByOwnerResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ISportCenterRepository _sportCenterRepository;
        private readonly IOutboxService _outboxService;
        private readonly IApplicationDbContext _dbContext;

        public CancelBookingByOwnerCommandHandler(
            IBookingRepository bookingRepository,
            ICourtRepository courtRepository,
            ISportCenterRepository sportCenterRepository,
            IOutboxService outboxService,
            IApplicationDbContext dbContext)
        {
            _bookingRepository = bookingRepository;
            _courtRepository = courtRepository;
            _sportCenterRepository = sportCenterRepository;
            _outboxService = outboxService;
            _dbContext = dbContext;
        }

        public async Task<CancelBookingByOwnerResult> Handle(CancelBookingByOwnerCommand request, CancellationToken cancellationToken)
        {
            // Get the booking by ID
            var booking = await _bookingRepository.GetBookingByIdAsync(BookingId.Of(request.BookingId), cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException($"Booking with ID {request.BookingId} not found");
            }

            // Check if the booking is already cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                throw new InvalidOperationException("The booking is already cancelled");
            }

            // Verify the owner has permission to cancel this booking
            var bookingDetails = await _bookingRepository.GetBookingDetailsAsync(booking.Id, cancellationToken);
            if (bookingDetails == null || !bookingDetails.Any())
            {
                throw new InvalidOperationException("The booking has no court details");
            }

            Guid? courtId = bookingDetails.First().CourtId?.Value;
            if (courtId == null)
            {
                throw new InvalidOperationException("The booking has invalid court information");
            }

            // Check if the requester is the owner of the court
            bool isAuthorized = false;
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(courtId.Value), cancellationToken);
            if (court != null && court.SportCenterId != null)
            {
                try
                {
                    var isSportCenterOwner = await _sportCenterRepository.IsOwnedByUserAsync(
                        court.SportCenterId.Value, request.OwnerId, cancellationToken);

                    if (isSportCenterOwner)
                        isAuthorized = true;
                }
                catch (NotFoundException)
                {
                    // Fallback check
                    try
                    {
                        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(
                            court.SportCenterId, cancellationToken);

                        if (sportCenter != null && sportCenter.OwnerId.Value == request.OwnerId)
                            isAuthorized = true;
                    }
                    catch
                    {
                        // If still can't find sport center, continue to unauthorized check
                    }
                }
            }

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("You don't have permission to cancel this booking");
            }

            // Begin transaction
            using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                // For owner cancellations, refund 100% of the paid amount
                decimal refundAmount = booking.TotalPrice - booking.RemainingBalance;

                // Update booking status and cancellation reason
                booking.Cancel();
                booking.SetCancellationReason(request.CancellationReason);
                booking.SetCancellationTime(request.RequestedAt);

                // Save changes to the database
                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);

                // Get the sport center owner
                var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(court.SportCenterId, cancellationToken);
                var sportCenterOwnerId = sportCenter.OwnerId.Value;

                // Create a booking cancelled refund event with 100% refund
                var bookingCancelledRefundEvent = new BookingCancelledRefundEvent(
                    booking.Id.Value,
                    booking.UserId.Value,
                    sportCenterOwnerId,
                    refundAmount,
                    $"[OWNER CANCELLED] {request.CancellationReason}",
                    request.RequestedAt);

                await _outboxService.SaveMessageAsync(bookingCancelledRefundEvent);

                // Also save notification event for other systems
                var bookingCancelledNotificationEvent = new BookingCancelledNotificationEvent(
                    booking.Id.Value,
                    booking.UserId.Value,
                    court.SportCenterId.Value,
                    true, // Always refund when owner cancels
                    refundAmount,
                    $"[OWNER CANCELLED] {request.CancellationReason}",
                    request.RequestedAt);

                await _outboxService.SaveMessageAsync(bookingCancelledNotificationEvent);

                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);

                // Return result
                return new CancelBookingByOwnerResult(
                    booking.Id.Value,
                    BookingStatus.Cancelled.ToString(),
                    refundAmount,
                    "Booking cancelled by owner with 100% refund"
                );
            }
            catch (Exception)
            {
                // Rollback the transaction if an error occurred
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
using BuildingBlocks.CQRS;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.BookingManagement.Command.UpdateBookingStatus
{
    public class UpdateBookingStatusHandler : ICommandHandler<UpdateBookingStatusCommand, UpdateBookingStatusResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ISportCenterRepository _sportCenterRepository;

        public UpdateBookingStatusHandler(
            IBookingRepository bookingRepository,
            ICourtRepository courtRepository,
            ISportCenterRepository sportCenterRepository)
        {
            _bookingRepository = bookingRepository;
            _courtRepository = courtRepository;
            _sportCenterRepository = sportCenterRepository;
        }

        public async Task<UpdateBookingStatusResult> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
        {
            // Get the booking
            var bookingId = BookingId.Of(request.BookingId);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return new UpdateBookingStatusResult(false, "Booking not found");
            }

            // Verify court owner permissions
            var ownerId = OwnerId.Of(request.OwnerId);
            var authorized = false;

            // Get all courts referenced in booking details
            foreach (var detail in booking.BookingDetails)
            {
                var court = await _courtRepository.GetCourtByIdAsync(detail.CourtId, cancellationToken);
                if (court == null) continue;

                var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(court.SportCenterId, cancellationToken);
                if (sportCenter == null) continue;

                if (sportCenter.OwnerId == ownerId)
                {
                    authorized = true;
                    break;
                }
            }

            if (!authorized)
            {
                return new UpdateBookingStatusResult(false, "You are not authorized to update this booking");
            }

            // Parse the requested status
            if (!Enum.TryParse<BookingStatus>(request.Status.ToString(), true, out var newStatus))
            {
                return new UpdateBookingStatusResult(false, $"Invalid status: {request.Status}");
            }

            // Validate status transitions
            if (!IsValidStatusTransition(booking.Status, newStatus))
            {
                return new UpdateBookingStatusResult(false, $"Invalid status transition from {booking.Status} to {newStatus}");
            }

            // Update booking status
            booking.UpdateStatus(newStatus);
            booking.SetLastModified(DateTime.UtcNow);

            await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);

            return new UpdateBookingStatusResult(true);
        }

        private bool IsValidStatusTransition(BookingStatus currentStatus, BookingStatus newStatus)
        {
            // Define valid status transitions
            switch (currentStatus)
            {
                case BookingStatus.PendingPayment:
                    return newStatus == BookingStatus.Deposited ||
                           newStatus == BookingStatus.Cancelled ||
                           newStatus == BookingStatus.PaymentFail;

                case BookingStatus.Deposited:
                    return newStatus == BookingStatus.Completed ||
                           newStatus == BookingStatus.Cancelled;

                // Can't change these final states
                case BookingStatus.Completed:
                case BookingStatus.Cancelled:
                case BookingStatus.PaymentFail:
                    return newStatus == BookingStatus.Cancelled;

                default:
                    return false;
            }
        }
    }
}
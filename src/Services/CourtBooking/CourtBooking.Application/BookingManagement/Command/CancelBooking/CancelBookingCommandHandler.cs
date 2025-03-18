using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Application.Data.Repositories;
using BuildingBlocks.Messaging.Outbox;
using BuildingBlocks.Exceptions;
using CourtBooking.Domain.Enums;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Domain.Models;
using BuildingBlocks.Messaging.Events;

namespace CourtBooking.Application.BookingManagement.Command.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ISportCenterRepository _sportCenterRepository;
        private readonly IOutboxService _outboxService;
        private readonly IApplicationDbContext _dbContext;

        public CancelBookingCommandHandler(
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

        public async Task<CancelBookingResult> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // Get booking
            var booking = await _bookingRepository.GetBookingByIdAsync(BookingId.Of(request.BookingId), cancellationToken);
            if (booking == null)
                throw new NotFoundException($"Booking with ID {request.BookingId} not found");

            // Check authorization
            bool isAuthorized = false;

            // User is the booking owner
            if (booking.UserId == UserId.Of(request.UserId))
                isAuthorized = true;

            // Lấy thông tin các booking details
            var bookingDetails = await _bookingRepository.GetBookingDetailsAsync(booking.Id, cancellationToken);
            if (!bookingDetails.Any())
                throw new NotFoundException("Booking details not found");

            // Lấy courtId từ booking detail đầu tiên
            var firstDetail = bookingDetails.First();
            var courtId = firstDetail.CourtId;

            // User is the court owner
            if (!isAuthorized && request.Role == "CourtOwner")
            {
                var courtInfo = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
                if (courtInfo != null)
                {
                    var isSportCenterOwner = await _sportCenterRepository.IsOwnedByUserAsync(
                        courtInfo.SportCenterId.Value, request.UserId);

                    if (isSportCenterOwner)
                        isAuthorized = true;
                }
            }

            if (!isAuthorized)
                throw new UnauthorizedAccessException("You don't have permission to cancel this booking");

            // Check if booking is already cancelled
            if (booking.Status == BookingStatus.Cancelled)
                throw new InvalidOperationException("Booking is already cancelled");

            // Get the court details for cancellation policy
            var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
            if (court == null)
                throw new NotFoundException($"Court with ID {courtId} not found");

            // Calculate refund amount based on cancellation policy
            decimal refundAmount = 0;
            var firstBookingTime = bookingDetails.Min(d => d.StartTime);

            // Tính khoảng thời gian giữa thời điểm hủy và thời điểm bắt đầu booking
            // Chuyển DateTime sang TimeSpan tính từ thời điểm hiện tại
            DateTime bookingStartDateTime = booking.BookingDate.Add(firstBookingTime);
            TimeSpan timeDifference = bookingStartDateTime - request.RequestedAt;

            if (timeDifference.TotalHours >= court.CancellationWindowHours)
            {
                // Eligible for refund
                refundAmount = booking.TotalPrice * (court.RefundPercentage / 100);
            }

            // Bắt đầu transaction
            var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                // Update booking status - phương thức Cancel có thể cần được thêm vào Booking entity
                booking.UpdateStatus(BookingStatus.Cancelled);
                booking.SetCancellationReason(request.CancellationReason);
                booking.SetCancellationTime(request.RequestedAt);

                await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);

                // Save changes to the booking
                await _dbContext.SaveChangesAsync(cancellationToken);

                // If refund is applicable, publish event for payment service
                if (refundAmount > 0)
                {
                    var refundEvent = new BookingCancelledRefundEvent(
                        booking.Id.Value,
                        booking.UserId.Value,
                        refundAmount,
                        request.CancellationReason,
                        request.RequestedAt
                    );

                    await _outboxService.SaveMessageAsync(refundEvent);
                }

                // Publish notification event
                var notificationEvent = new BookingCancelledNotificationEvent(
                    booking.Id.Value,
                    booking.UserId.Value,
                    court.SportCenterId.Value,
                    refundAmount > 0,
                    refundAmount,
                    request.CancellationReason,
                    request.RequestedAt
                );

                await _outboxService.SaveMessageAsync(notificationEvent);

                await transaction.CommitAsync(cancellationToken);

                // Return result
                string message = refundAmount > 0
                    ? "Booking cancelled successfully. Partial refund processed."
                    : "Booking cancelled successfully. No refund applicable.";

                return new CancelBookingResult(
                    booking.Id.Value,
                    "cancelled",
                    refundAmount,
                    message
                );
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
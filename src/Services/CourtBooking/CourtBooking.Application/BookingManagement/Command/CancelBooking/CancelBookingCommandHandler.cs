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
using BuildingBlocks.Messaging;
using CourtBooking.Application.Data;
using CourtBooking.Domain.Events;

namespace CourtBooking.Application.BookingManagement.Command.CancelBooking;

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

        // Check if the user is authorized to cancel this booking
        bool isAuthorized = false;

        // User is the booking owner
        if (booking.UserId == UserId.Of(request.UserId))
        {
            isAuthorized = true;
        }

        // Get court information from booking details
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

        // User is the court owner or sport center owner
        if (!isAuthorized && (request.Role == "SportCenterOwner" || request.Role == "CourtOwner"))
        {
            var courtInfo = await _courtRepository.GetCourtByIdAsync(CourtId.Of(courtId.Value), cancellationToken);
            if (courtInfo != null && courtInfo.SportCenterId != null)
            {
                try
                {
                    var isSportCenterOwner = await _sportCenterRepository.IsOwnedByUserAsync(
                        courtInfo.SportCenterId.Value, request.UserId, cancellationToken);

                    if (isSportCenterOwner)
                        isAuthorized = true;
                }
                catch (NotFoundException)
                {
                    // For test scenarios, if the sport center repository throws a not found exception,
                    // we'll check the sport center directly with a different method
                    try
                    {
                        var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(
                            courtInfo.SportCenterId, cancellationToken);

                        if (sportCenter != null && sportCenter.OwnerId.Value == request.UserId)
                            isAuthorized = true;
                    }
                    catch
                    {
                        // If still can't find sport center, just continue
                        // (The unauthorized check below will handle this)
                    }
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
            // Calculate refund amount (if applicable)
            decimal refundAmount = await CalculateRefundAmount(booking, bookingDetails.First(), cancellationToken);

            // Update booking status and cancellation reason
            booking.Cancel();
            booking.SetCancellationReason(request.CancellationReason);
            booking.SetCancellationTime(request.RequestedAt);

            // Save changes to the database
            await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);

            // Get the SportCenterOwnerId from the booking
            var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(courtId.Value), cancellationToken);
            var sportCenter = await _sportCenterRepository.GetSportCenterByIdAsync(court.SportCenterId, cancellationToken);
            var sportCenterOwnerId = sportCenter.OwnerId.Value;

            // Save the integration event to the outbox instead of multiple domain events
            var bookingCancelledRefundEvent = new BookingCancelledRefundEvent(
                booking.Id.Value,
                booking.UserId.Value,
                sportCenterOwnerId,
                refundAmount,
                request.CancellationReason,
                request.RequestedAt);

            await _outboxService.SaveMessageAsync(bookingCancelledRefundEvent);

            // Also save notification event for other systems
            var bookingCancelledNotificationEvent = new BookingCancelledNotificationEvent(
                booking.Id.Value,
                booking.UserId.Value,
                court.SportCenterId.Value,
                refundAmount > 0,
                refundAmount,
                request.CancellationReason,
                request.RequestedAt);

            await _outboxService.SaveMessageAsync(bookingCancelledNotificationEvent);

            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);

            // Return result
            return new CancelBookingResult(
                booking.Id.Value,
                BookingStatus.Cancelled.ToString(),
                refundAmount,
                "Booking cancelled successfully"
            );
        }
        catch (Exception)
        {
            // Rollback the transaction if an error occurred
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // Helper method to calculate refund amount based on business rules
    private async Task<decimal> CalculateRefundAmount(Booking booking, BookingDetail bookingDetail, CancellationToken cancellationToken)
    {
        // If nothing was paid yet, no refund
        if (booking.TotalPaid <= 0)
            return 0;

        // Get a court to check cancellation policy
        var courtId = bookingDetail.CourtId.Value;
        var court = await _courtRepository.GetCourtByIdAsync(CourtId.Of(courtId), cancellationToken);

        // Calculate time remaining until booking starts
        var now = DateTime.UtcNow;
        var bookingTime = booking.BookingDate.Add(bookingDetail.StartTime);
        var hoursRemaining = (bookingTime - now).TotalHours;

        // Check if cancellation is within the window for refund
        if (court != null && hoursRemaining >= court.CancellationWindowHours)
        {
            // Apply refund percentage
            var refundPercentage = court.RefundPercentage / 100m;
            return booking.TotalPaid * refundPercentage;
        }

        // Default: no refund outside of cancellation window
        return 0;
    }
}
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.BookingManagement.Commands.CreateBooking;

public class CreateBookingHandler(IApplicationDbContext _context)
    : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{

    public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.Of(request.Booking.UserId);
        var courtId = CourtId.Of(request.Booking.CourtId);
        var promotionId = request.Booking.PromotionId.HasValue ? PromotionId.Of(request.Booking.PromotionId.Value) : null;

        var booking = Booking.Create(
            userId,
            courtId,
            request.Booking.BookingDate,
            request.Booking.StartTime,
            request.Booking.EndTime,
            promotionId
        );

        // Calculate total price based on court schedules
        var courtSchedules = await _context.CourtSlots
            .Where(cs => cs.CourtId == courtId)
            .ToListAsync(cancellationToken);

        var filteredCourtSchedules = courtSchedules
            .Where(cs => cs.DayOfWeek.Days.Contains((int)request.Booking.BookingDate.DayOfWeek))
            .ToList();

        booking.CalculateTotalPrice(filteredCourtSchedules);

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateBookingResult(booking.Id.Value);
    }
}

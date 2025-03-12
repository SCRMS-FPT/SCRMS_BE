using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Models;
using CourtBooking.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CourtBooking.Domain.Exceptions;

namespace CourtBooking.Application.BookingManagement.Commands.CreateBooking
{
    public class CreateBookingHandler(IApplicationDbContext _context) : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.Booking.UserId);
            var promotionId = request.Booking.PromotionId.HasValue ?
                             PromotionId.Of(request.Booking.PromotionId.Value) : null;

            var booking = Booking.Create(
                userId,
                request.Booking.BookingDate,
                request.Booking.Note,
                promotionId
            );

            foreach (var detail in request.Booking.BookingDetails)
            {
                var courtId = CourtId.Of(detail.CourtId);

                // Getting the day of the week as an integer (1-7)
                var bookingDayOfWeekInt = (int)request.Booking.BookingDate.DayOfWeek + 1;

                // First, get all schedules for the court
                var allCourtSchedules = await _context.CourtSchedules
                    .AsNoTracking()
                    .Where(s => s.CourtId == courtId)
                    .ToListAsync(cancellationToken);

                // Then filter in-memory for the correct day of week
                var schedules = allCourtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                    .ToList();

                if (!schedules.Any())
                {
                    throw new ApplicationException($"No schedules found for court {courtId.Value} on day {bookingDayOfWeekInt}");
                }

                booking.AddBookingDetail(
                    courtId,
                    detail.StartTime,
                    detail.EndTime,
                    schedules
                );
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateBookingResult(booking.Id.Value);
        }
    }
}

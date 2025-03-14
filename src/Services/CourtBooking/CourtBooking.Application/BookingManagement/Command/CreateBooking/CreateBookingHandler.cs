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
using CourtBooking.Application.Data.Repositories;

namespace CourtBooking.Application.BookingManagement.Command.CreateBooking
{
    public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICourtScheduleRepository _courtScheduleRepository;

        public CreateBookingHandler(
            IBookingRepository bookingRepository,
            ICourtScheduleRepository courtScheduleRepository)
        {
            _bookingRepository = bookingRepository;
            _courtScheduleRepository = courtScheduleRepository;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.Booking.UserId);
            var booking = Booking.Create(userId, request.Booking.BookingDate, request.Booking.Note);

            foreach (var detail in request.Booking.BookingDetails)
            {
                var courtId = CourtId.Of(detail.CourtId);
                var bookingDayOfWeekInt = (int)request.Booking.BookingDate.DayOfWeek + 1;
                var allCourtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
                var schedules = allCourtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                    .ToList();
                if (!schedules.Any())
                {
                    throw new ApplicationException($"No schedules found for court {courtId.Value} on day {bookingDayOfWeekInt}");
                }
                booking.AddBookingDetail(courtId, detail.StartTime, detail.EndTime, schedules);
            }

            await _bookingRepository.AddBookingAsync(booking, cancellationToken);
            return new CreateBookingResult(booking.Id.Value);
        }
    }
}
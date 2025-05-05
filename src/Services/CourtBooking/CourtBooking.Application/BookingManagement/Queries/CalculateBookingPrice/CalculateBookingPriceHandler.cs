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
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Queries.CalculateBookingPrice
{
    public class CalculateBookingPriceHandler : IRequestHandler<CalculateBookingPriceQuery, CalculateBookingPriceResult>
    {
        private readonly ICourtScheduleRepository _courtScheduleRepository;
        private readonly ICourtRepository _courtRepository;
        private readonly ICourtPromotionRepository _courtPromotionRepository;

        public CalculateBookingPriceHandler(
            ICourtScheduleRepository courtScheduleRepository,
            ICourtRepository courtRepository,
            ICourtPromotionRepository courtPromotionRepository)
        {
            _courtScheduleRepository = courtScheduleRepository;
            _courtRepository = courtRepository;
            _courtPromotionRepository = courtPromotionRepository;
        }

        public async Task<CalculateBookingPriceResult> Handle(CalculateBookingPriceQuery request, CancellationToken cancellationToken)
        {
            var userId = UserId.Of(request.UserId);
            var courtPriceDetails = new List<CourtPriceDetailDTO>();
            decimal totalPrice = 0;
            decimal minimumDeposit = 0;

            foreach (var detail in request.Booking.BookingDetails)
            {
                var courtId = CourtId.Of(detail.CourtId);
                var court = await _courtRepository.GetCourtByIdAsync(courtId, cancellationToken);
                if (court == null)
                {
                    throw new ApplicationException($"Court {detail.CourtId} not found");
                }

                // Get court schedules for the booking day
                var bookingDayOfWeek = request.Booking.BookingDate.DayOfWeek;
                var bookingDayOfWeekInt = bookingDayOfWeek == DayOfWeek.Sunday ? 7 : (int)bookingDayOfWeek;
                var allCourtSchedules = await _courtScheduleRepository.GetSchedulesByCourtIdAsync(courtId, cancellationToken);
                var schedules = allCourtSchedules
                    .Where(s => s.DayOfWeek.Days.Contains(bookingDayOfWeekInt))
                    .ToList();

                if (!schedules.Any())
                {
                    throw new ApplicationException($"No schedules found for court {courtId.Value} on day {bookingDayOfWeekInt}");
                }

                // Calculate original price
                decimal originalPrice = CalculatePrice(detail.StartTime, detail.EndTime, schedules);

                // Get applicable promotions
                var validPromotions = await _courtPromotionRepository.GetValidPromotionsForCourtAsync(
                    courtId,
                    request.Booking.BookingDate,
                    request.Booking.BookingDate,
                    cancellationToken);

                // Find the best promotion for the customer
                var bestPromotion = validPromotions
                    .OrderByDescending(p => p.DiscountType.ToLower() == "percentage" ?
                        p.DiscountValue :
                        p.DiscountValue / 100)
                    .FirstOrDefault();

                // Calculate discounted price
                decimal discountedPrice = originalPrice;
                if (bestPromotion != null)
                {
                    if (bestPromotion.DiscountType.ToLower() == "percentage")
                    {
                        discountedPrice = originalPrice * (1 - (bestPromotion.DiscountValue / 100));
                    }
                    else if (bestPromotion.DiscountType.ToLower() == "fixed")
                    {
                        discountedPrice = Math.Max(0, originalPrice - bestPromotion.DiscountValue);
                    }
                }

                // Calculate minimum deposit for this court
                decimal courtMinDeposit = discountedPrice * (court.MinDepositPercentage / 100);

                // Add to totals
                totalPrice += discountedPrice;
                minimumDeposit += courtMinDeposit;

                // Add court price details
                courtPriceDetails.Add(new CourtPriceDetailDTO(
                    CourtId: court.Id.Value,
                    CourtName: court.CourtName.Value,
                    StartTime: detail.StartTime,
                    EndTime: detail.EndTime,
                    OriginalPrice: originalPrice,
                    DiscountedPrice: discountedPrice,
                    PromotionName: bestPromotion?.Description,
                    DiscountType: bestPromotion?.DiscountType,
                    DiscountValue: bestPromotion?.DiscountValue
                ));
            }

            return new CalculateBookingPriceResult(
                CourtPrices: courtPriceDetails,
                TotalPrice: totalPrice,
                MinimumDeposit: Math.Round(minimumDeposit, 2)
            );
        }

        private decimal CalculatePrice(TimeSpan startTime, TimeSpan endTime, List<CourtSchedule> schedules)
        {
            decimal total = 0;
            TimeSpan current = startTime;

            // Sort schedules by StartTime for easier processing
            var sortedSchedules = schedules.OrderBy(s => s.StartTime).ToList();

            while (current < endTime)
            {
                // Find the schedule that covers the current time
                var schedule = sortedSchedules.FirstOrDefault(s => s.StartTime <= current && s.EndTime > current);
                if (schedule == null)
                    throw new ApplicationException($"No schedule found for time slot starting at {current}");

                // Determine how long we stay in this schedule period
                TimeSpan slotEndTime = schedule.EndTime < endTime ? schedule.EndTime : endTime;
                TimeSpan duration = slotEndTime - current;

                // Calculate price for this period using hourly rate
                decimal hourlyRate = schedule.PriceSlot;
                decimal hoursUsed = (decimal)duration.TotalHours;
                total += hourlyRate * hoursUsed;

                // Move to next time slot
                current = slotEndTime;
            }

            return total;
        }
    }
}
using System;

namespace CourtBooking.Application.DTOs;

public record CourtScheduleUpdateDTO(
    Guid Id,
    int[] DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal PriceSlot,
    int Status
);

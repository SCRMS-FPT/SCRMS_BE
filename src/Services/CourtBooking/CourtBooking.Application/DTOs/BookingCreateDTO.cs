using System;

namespace CourtBooking.Application.DTOs;

public record BookingCreateDTO(
    Guid UserId,
    Guid CourtId,
    DateTime BookingDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    Guid? PromotionId
);

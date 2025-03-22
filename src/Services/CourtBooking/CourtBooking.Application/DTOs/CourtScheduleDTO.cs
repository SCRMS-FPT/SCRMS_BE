namespace CourtBooking.Application.DTOs;

public record CourtScheduleDTO(
    Guid Id,
    Guid CourtId,
    int[] DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal PriceSlot,
    int Status,
    DateTime CreatedAt,
    DateTime? LastModified
);
public record BookingDto(
    Guid Id,
    Guid UserId,
    DateTime BookingDate,
    decimal TotalTime,
    decimal TotalPrice,
    string Status,
    string? Note,
    DateTime CreatedAt,
    DateTime? LastModified,
    List<BookingDetailDto> BookingDetails
);

public record BookingDetailDto(
    Guid Id,
    Guid CourtId,
    string CourtName,
    string SportsCenterName,
    string StartTime,
    string EndTime,
    decimal TotalPrice
);
using CourtBooking.Domain.Enums;

public record BookingDto(
    Guid Id,
    Guid UserId,
    decimal TotalTime,
    decimal TotalPrice,
    decimal RemainingBalance, // Hiển thị số tiền còn lại
    decimal InitialDeposit, // Hiển thị số tiền đặt cọc ban đầu
    string Status,
    DateTime BookingDate,
    string Note,
    DateTime CreatedAt,
    DateTime? LastModified,
    List<BookingDetailDto> BookingDetails);

public record BookingDetailDto(
    Guid Id,
    Guid CourtId,
    string CourtName,
    string SportsCenterName,
    string StartTime,
    string EndTime,
    decimal TotalPrice
);

public record CancelBookingRequest(
    string CancellationReason,
    DateTime RequestedAt
);
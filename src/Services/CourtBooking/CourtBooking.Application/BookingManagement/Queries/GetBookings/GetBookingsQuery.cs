using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Queries.GetBookings;

public record GetBookingsQuery(
    Guid UserId,
    string Role,
    string? ViewAs,
    Guid? FilterUserId,
    Guid? CourtId,
    Guid? SportsCenterId,
    BookingStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page,
    int Limit
) : IRequest<GetBookingsResult>;

public record GetBookingsResult(List<BookingDto> Bookings, int TotalCount);
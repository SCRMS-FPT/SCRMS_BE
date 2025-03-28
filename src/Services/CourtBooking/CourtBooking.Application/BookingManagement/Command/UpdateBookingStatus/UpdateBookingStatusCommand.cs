using BuildingBlocks.CQRS;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Command.UpdateBookingStatus
{
    // Update the command to use enum instead of string
    public record UpdateBookingStatusCommand(
        Guid BookingId,
        Guid OwnerId,
        BookingStatus Status
    ) : ICommand<UpdateBookingStatusResult>;

    public record UpdateBookingStatusResult(bool IsSuccess, string? ErrorMessage = null);
}
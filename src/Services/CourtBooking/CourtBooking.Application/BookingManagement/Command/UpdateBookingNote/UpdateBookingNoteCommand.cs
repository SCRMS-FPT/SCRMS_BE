using BuildingBlocks.CQRS;

namespace CourtBooking.Application.BookingManagement.Command.UpdateBookingNote
{
    public record UpdateBookingNoteCommand(
        Guid BookingId,
        Guid UserId,
        string Note
    ) : ICommand<UpdateBookingNoteResult>;

    public record UpdateBookingNoteResult(bool IsSuccess, string? ErrorMessage = null);
}
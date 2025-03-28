using BuildingBlocks.CQRS;
using CourtBooking.Application.Data.Repositories;
using CourtBooking.Domain.ValueObjects;

namespace CourtBooking.Application.BookingManagement.Command.UpdateBookingNote
{
    public class UpdateBookingNoteHandler : ICommandHandler<UpdateBookingNoteCommand, UpdateBookingNoteResult>
    {
        private readonly IBookingRepository _bookingRepository;

        public UpdateBookingNoteHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<UpdateBookingNoteResult> Handle(UpdateBookingNoteCommand request, CancellationToken cancellationToken)
        {
            // Get the booking
            var bookingId = BookingId.Of(request.BookingId);
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId, cancellationToken);

            if (booking == null)
            {
                return new UpdateBookingNoteResult(false, "Booking not found");
            }

            // Verify that the user owns this booking
            if (booking.UserId.Value != request.UserId)
            {
                return new UpdateBookingNoteResult(false, "You are not authorized to update this booking note");
            }

            // Update the note
            booking.UpdateNote(request.Note);
            await _bookingRepository.UpdateBookingAsync(booking, cancellationToken);
            return new UpdateBookingNoteResult(true);
        }
    }
}
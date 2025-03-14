using MediatR;
using CourtBooking.Application.DTOs;

namespace CourtBooking.Application.BookingManagement.Command.CreateBooking
{
    public record CreateBookingCommand(BookingCreateDTO Booking) : IRequest<CreateBookingResult>;

    public record CreateBookingResult(Guid Id);
}
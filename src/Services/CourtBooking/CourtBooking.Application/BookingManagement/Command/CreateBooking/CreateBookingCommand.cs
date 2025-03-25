using MediatR;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Command.CreateBooking
{

    public record CreateBookingCommand(Guid UserId, BookingCreateDTO Booking) : IRequest<CreateBookingResult>;

    public record CreateBookingResult(Guid Id, string Status);
}
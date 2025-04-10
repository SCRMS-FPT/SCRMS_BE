using MediatR;
using CourtBooking.Application.DTOs;
using CourtBooking.Domain.Enums;

namespace CourtBooking.Application.BookingManagement.Command.CreateOwnerBooking
{
    public record CreateOwnerBookingCommand(
        Guid OwnerId,
        BookingCreateDTO Booking,
        string Note = "Đặt trực tiếp tại sân") : IRequest<CreateOwnerBookingResult>;

    public record CreateOwnerBookingResult(
        Guid Id,
        string Status,
        bool Success,
        string Message = "");
}
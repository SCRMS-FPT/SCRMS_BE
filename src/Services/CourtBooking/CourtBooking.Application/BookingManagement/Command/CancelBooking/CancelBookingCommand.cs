using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.BookingManagement.Command.CancelBooking
{
    public record CancelBookingCommand(
        Guid BookingId,
        string CancellationReason,
        DateTime RequestedAt,
        Guid UserId,
        string Role
    ) : IRequest<CancelBookingResult>;

    public record CancelBookingResult(
        Guid BookingId,
        string Status,
        decimal RefundAmount,
        string Message
    );

    public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
    {
        public CancelBookingCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
            RuleFor(c => c.CancellationReason).NotEmpty().MaximumLength(500);
            RuleFor(c => c.RequestedAt).NotEmpty();
            RuleFor(c => c.UserId).NotEmpty();
        }
    }
}
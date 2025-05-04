using FluentValidation;
using MediatR;
using System;

namespace CourtBooking.Application.BookingManagement.Command.CancelBookingByOwner
{
    public record CancelBookingByOwnerCommand(
        Guid BookingId,
        string CancellationReason,
        DateTime RequestedAt,
        Guid OwnerId
    ) : IRequest<CancelBookingByOwnerResult>;

    public record CancelBookingByOwnerResult(
        Guid BookingId,
        string Status,
        decimal RefundAmount,
        string Message
    );

    public class CancelBookingByOwnerCommandValidator : AbstractValidator<CancelBookingByOwnerCommand>
    {
        public CancelBookingByOwnerCommandValidator()
        {
            RuleFor(c => c.BookingId).NotEmpty();
            RuleFor(c => c.CancellationReason).NotEmpty().MaximumLength(500);
            RuleFor(c => c.RequestedAt).NotEmpty();
            RuleFor(c => c.OwnerId).NotEmpty();
        }
    }
}

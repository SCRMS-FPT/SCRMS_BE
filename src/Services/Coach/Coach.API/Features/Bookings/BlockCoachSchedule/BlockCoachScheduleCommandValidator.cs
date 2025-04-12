using FluentValidation;

namespace Coach.API.Features.Bookings.BlockCoachSchedule
{
    public class BlockCoachScheduleCommandValidator : AbstractValidator<BlockCoachScheduleCommand>
    {
        public BlockCoachScheduleCommandValidator()
        {
            RuleFor(x => x.CoachId)
                .NotEmpty().WithMessage("Coach ID is required.");

            RuleFor(x => x.SportId)
                .NotEmpty().WithMessage("Sport ID is required.");

            RuleFor(x => x.BookingDate)
                .NotEmpty().WithMessage("Booking date is required.");

            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("Start time must be earlier than end time.");
        }
    }
}
using FluentValidation;

namespace CourtBooking.Application.CourtManagement.Command.CreateCourtSchedule;

public record CreateCourtScheduleCommand(
    Guid CourtId,
    int[] DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    decimal PriceSlot
) : ICommand<CreateCourtScheduleResult>;

public record CreateCourtScheduleResult(Guid Id);

public class CreateCourtScheduleCommandValidator : AbstractValidator<CreateCourtScheduleCommand>
{
    public CreateCourtScheduleCommandValidator()
    {
        RuleFor(x => x.CourtId).NotEmpty();
        RuleFor(x => x.DayOfWeek).NotEmpty().Must(days => days.All(d => d >= 1 && d <= 7));
        RuleFor(x => x.StartTime).NotEmpty();
        RuleFor(x => x.EndTime).NotEmpty().GreaterThan(x => x.StartTime);
        RuleFor(x => x.PriceSlot).GreaterThanOrEqualTo(0);
    }
}
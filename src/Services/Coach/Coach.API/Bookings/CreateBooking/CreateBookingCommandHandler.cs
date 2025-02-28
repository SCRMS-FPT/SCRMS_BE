using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.CreateBooking
{
    public record CreateBookingCommand(
        Guid UserId,
        Guid CoachId,
        Guid SportId,
        DateOnly BookingDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        Guid? PackageId
    ) : ICommand<CreateBookingResult>;

    public record CreateBookingResult(Guid Id);

    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

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

    internal class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly CoachDbContext context;
        private readonly IMediator mediator;

        public CreateBookingCommandHandler(CoachDbContext context, IMediator mediator)
        {
            this.context = context;
            this.mediator = mediator;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.UserId == command.CoachId, cancellationToken);

            if (coach == null)
                throw new Exception("Coach not found");

            var dayOfWeek = (int)command.BookingDate.DayOfWeek + 1;
            var schedules = await context.CoachSchedules
                .Where(s => s.CoachId == command.CoachId && s.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            var isValidTime = schedules.Any(s =>
                command.StartTime >= s.StartTime && command.EndTime <= s.EndTime);

            if (!isValidTime)
                throw new Exception("Booking time is outside coach's available hours");

            var overlappingBookings = await context.CoachBookings
                .Where(b => b.CoachId == command.CoachId &&
                            b.BookingDate == command.BookingDate &&
                            b.StartTime < command.EndTime &&
                            b.EndTime > command.StartTime)
                .AnyAsync(cancellationToken);

            if (overlappingBookings)
                throw new Exception("The selected time slot is already booked");

            var duration = (command.EndTime - command.StartTime).TotalHours;
            var totalPrice = coach.RatePerHour * (decimal)duration;

            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CoachId = command.CoachId,
                SportId = command.SportId,
                BookingDate = command.BookingDate,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                Status = "pending",
                TotalPrice = totalPrice,
                PackageId = command.PackageId,
                CreatedAt = DateTime.UtcNow
            };

            context.CoachBookings.Add(booking);
            await context.SaveChangesAsync(cancellationToken);

            await mediator.Publish(new BookingCreatedEvent(booking.Id, booking.UserId, booking.CoachId), cancellationToken);

            return new CreateBookingResult(booking.Id);
        }
    }

    public record BookingCreatedEvent(Guid BookingId, Guid UserId, Guid CoachId) : INotification;

    public class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
    {
        public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Booking {notification.BookingId} created. User: {notification.UserId}, Coach: {notification.CoachId}");
        }
    }
}
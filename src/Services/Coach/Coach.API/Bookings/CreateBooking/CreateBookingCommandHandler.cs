using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Bookings.CreateBooking
{
    public record CreateBookingCommand(
        Guid UserId,
        Guid CoachId,
        Guid SportId,
        DateTime StartTime,
        DateTime EndTime
    ) : ICommand<CreateBookingResult>;

    public record CreateBookingResult(Guid Id);

    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CoachId).NotEmpty();
            RuleFor(x => x.SportId).NotEmpty();
            RuleFor(x => x.StartTime).LessThan(x => x.EndTime);
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
            // Check if the coach exists
            var coach = await context.Coaches
                .AnyAsync(c => c.UserId == command.CoachId, cancellationToken);

            if (!coach)
                throw new CoachNotFoundException(command.CoachId);

            // Check coach's schedule for the day of the week of the booking
            var dayOfWeek = (int)command.StartTime.DayOfWeek + 1; // Adjust if necessary (e.g., Sunday is 0 in DayOfWeek)
            var schedules = await context.CoachSchedules
                .Where(s => s.CoachId == command.CoachId && s.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            // Check if the booking time fits within any schedule
            var bookingStartTime = command.StartTime.TimeOfDay;
            var bookingEndTime = command.EndTime.TimeOfDay;

            var isValidTime = schedules.Any(s =>
                bookingStartTime >= s.StartTime &&
                bookingEndTime <= s.EndTime);

            if (!isValidTime)
                throw new InvalidBookingTimeException("Booking time is outside coach's available hours.");

            // Check for existing bookings that overlap
            var overlappingBookings = await context.CoachBookings
                .Where(b => b.CoachId == command.CoachId &&
                            b.StartTime < command.EndTime &&
                            b.EndTime > command.StartTime)
                .AnyAsync(cancellationToken);

            if (overlappingBookings)
                throw new BookingConflictException("The selected time slot is already booked.");

            // Create the new booking
            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CoachId = command.CoachId,
                SportId = command.SportId,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            // Add booking to the context and save changes
            context.CoachBookings.Add(booking);
            await context.SaveChangesAsync(cancellationToken);

            // Publish event for notification
            await mediator.Publish(new BookingCreatedEvent(booking.Id, booking.UserId, booking.CoachId), cancellationToken);

            return new CreateBookingResult(booking.Id);
        }
    }

    public class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
    {
        public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
        {
            // Send notification to user and coach
            // This could be an email service, push notification, etc.
            // For now, log it
            Console.WriteLine($"Booking {notification.BookingId} created. User: {notification.UserId}, Coach: {notification.CoachId}");
        }
    }
}
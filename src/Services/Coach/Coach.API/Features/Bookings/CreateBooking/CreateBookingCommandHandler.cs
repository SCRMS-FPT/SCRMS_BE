using Coach.API.Data;
using Coach.API.Data.Models;
using Coach.API.Data.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Bookings.CreateBooking
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

    public record CreateBookingResult(Guid Id, int SessionsRemaining);

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

    public class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly ICoachRepository _coachRepository;
        private readonly ICoachScheduleRepository _scheduleRepository;
        private readonly ICoachBookingRepository _bookingRepository;
        private readonly ICoachPackageRepository _packageRepository;
        private readonly CoachDbContext _context;
        private readonly IMediator _mediator;

        public CreateBookingCommandHandler(
            ICoachRepository coachRepository,
            ICoachScheduleRepository scheduleRepository,
            ICoachBookingRepository bookingRepository,
            ICoachPackageRepository packageRepository,
            CoachDbContext context,
            IMediator mediator)
        {
            _coachRepository = coachRepository;
            _scheduleRepository = scheduleRepository;
            _bookingRepository = bookingRepository;
            _packageRepository = packageRepository;
            _context = context;
            _mediator = mediator;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand command, CancellationToken cancellationToken)
        {
            var coach = await _coachRepository.GetCoachByIdAsync(command.CoachId, cancellationToken);
            if (coach == null)
                throw new Exception("Coach not found");

            var dayOfWeek = (int)command.BookingDate.DayOfWeek + 1;
            var schedules = await _scheduleRepository.GetCoachSchedulesByCoachIdAsync(command.CoachId, cancellationToken);
            var isValidTime = schedules.Any(s => s.DayOfWeek == dayOfWeek && command.StartTime >= s.StartTime && command.EndTime <= s.EndTime);
            if (!isValidTime)
                throw new Exception("Booking time is outside coach's available hours");

            var hasOverlap = await _bookingRepository.HasOverlappingCoachBookingAsync(
                command.CoachId,
                command.BookingDate,
                command.StartTime,
                command.EndTime,
                cancellationToken);
            if (hasOverlap)
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

            await _bookingRepository.AddCoachBookingAsync(booking, cancellationToken);
            int sessionsRemaining = 0;
            if (command.PackageId.HasValue)
            {
                var package = await _packageRepository.GetCoachPackageByIdAsync(command.PackageId.Value, cancellationToken);
                if (package == null)
                    throw new Exception("Package not found");

                // Logic xử lý package purchase nếu cần...
            }
            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new BookingCreatedEvent(booking.Id, booking.UserId, booking.CoachId), cancellationToken);

            return new CreateBookingResult(booking.Id, sessionsRemaining);
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
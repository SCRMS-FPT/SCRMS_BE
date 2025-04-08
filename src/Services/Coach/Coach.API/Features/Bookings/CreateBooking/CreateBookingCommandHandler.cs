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

            var dayOfWeek = command.BookingDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)command.BookingDate.DayOfWeek;
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
            string bookingStatus = "pending";
            int sessionsRemaining = 0;

            // Check if booking is using a package
            if (command.PackageId.HasValue)
            {
                var package = await _packageRepository.GetCoachPackageByIdAsync(command.PackageId.Value, cancellationToken);
                if (package == null)
                    throw new Exception("Package not found");

                // Check if the user has already purchased this package
                var packagePurchases = await _context.CoachPackagePurchases
                    .Where(p => p.UserId == command.UserId &&
                                p.CoachPackageId == command.PackageId.Value &&
                                p.ExpiryDate > DateTime.UtcNow)
                    .OrderByDescending(p => p.PurchaseDate)
                    .ToListAsync(cancellationToken);

                var validPurchase = packagePurchases.FirstOrDefault(p => p.SessionsUsed < package.SessionCount);

                if (validPurchase != null)
                {
                    // User has a valid package with remaining sessions
                    validPurchase.SessionsUsed += 1;
                    validPurchase.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);

                    // Calculate remaining sessions
                    sessionsRemaining = package.SessionCount - validPurchase.SessionsUsed;

                    // Set booking status to completed since it's pre-paid through the package
                    bookingStatus = "completed";
                }
                else
                {
                    // No valid package purchase or no remaining sessions
                    throw new Exception("No valid package with remaining sessions found. Please purchase a package first or select a different payment method.");
                }
            }

            var booking = new CoachBooking
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                CoachId = command.CoachId,
                SportId = command.SportId,
                BookingDate = command.BookingDate,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                Status = bookingStatus,
                TotalPrice = totalPrice,
                PackageId = command.PackageId,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.AddCoachBookingAsync(booking, cancellationToken);
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
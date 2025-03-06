using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Packages.CreatePackage;
using MediatR;
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

    internal class CreatePackageCommandHandler : ICommandHandler<CreatePackageCommand, CreatePackageResult>
    {
        private readonly ICoachPackageRepository _packageRepository;
        private readonly CoachDbContext _context;

        public CreatePackageCommandHandler(ICoachPackageRepository packageRepository, CoachDbContext context)
        {
            _packageRepository = packageRepository;
            _context = context;
        }

        public async Task<CreatePackageResult> Handle(
            CreatePackageCommand command,
            CancellationToken cancellationToken)
        {
            var package = new CoachPackage
            {
                Id = Guid.NewGuid(),
                CoachId = command.CoachId,
                Name = command.Name,
                Description = command.Description,
                Price = command.Price,
                SessionCount = command.SessionCount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _packageRepository.AddCoachPackageAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePackageResult(package.Id);
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
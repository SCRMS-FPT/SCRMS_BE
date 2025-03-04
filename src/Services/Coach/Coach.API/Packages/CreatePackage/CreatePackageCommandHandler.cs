using Coach.API.Bookings.CreateBooking;
using Coach.API.Data;
using Coach.API.Data.Repositories;

namespace Coach.API.Packages.CreatePackage
{
    public record CreatePackageResult(Guid Id);

    public record CreatePackageCommand(
    Guid CoachId,
    string Name,
    string Description,
    decimal Price,
    int SessionCount) : ICommand<CreatePackageResult>;

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
}
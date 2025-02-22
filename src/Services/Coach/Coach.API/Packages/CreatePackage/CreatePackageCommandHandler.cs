using Coach.API.Bookings.CreateBooking;
using Coach.API.Data;

namespace Coach.API.Packages.CreatePackage
{
    public record CreatePackageResult(Guid Id);

    public record CreatePackageCommand(
    Guid CoachId,
    string Name,
    string Description,
    decimal Price,
    int SessionCount) : ICommand<CreatePackageResult>;

    internal class CreatePackageCommandHandler(CoachDbContext context)
    : ICommandHandler<CreatePackageCommand, CreatePackageResult>
    {
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

            await context.CoachPackages.AddAsync(package, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return new CreatePackageResult(package.Id);
        }
    }
}
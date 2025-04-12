using Coach.API.Data;
using Coach.API.Data.Repositories;
using Coach.API.Features.Packages.GetActivePackages;
using BuildingBlocks.Exceptions;

namespace Coach.API.Features.Packages.UpdatePackage
{
    public record UpdatePackageResult(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int SessionCount,
        string Status
    );

    public record UpdatePackageCommand(
        Guid PackageId,
        Guid CoachId,
        string Name,
        string Description,
        decimal Price,
        int SessionCount,
        string Status
    ) : ICommand<UpdatePackageResult>;

    public class UpdatePackageCommandValidator : AbstractValidator<UpdatePackageCommand>
    {
        public UpdatePackageCommandValidator()
        {
            RuleFor(x => x.PackageId).NotEmpty().WithMessage("PackageId is required");
            RuleFor(x => x.CoachId).NotEmpty().WithMessage("CoachId is required");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name must not be empty");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
            RuleFor(x => x.SessionCount).GreaterThan(0).WithMessage("SessionCount must be greater than 0");
            RuleFor(x => x.Status).Must(status => status == "active" || status == "inactive")
                .WithMessage("Status must be either 'active' or 'inactive'");
        }
    }

    public class UpdatePackageCommandHandler : ICommandHandler<UpdatePackageCommand, UpdatePackageResult>
    {
        private readonly ICoachPackageRepository _packageRepository;
        private readonly CoachDbContext _context;

        public UpdatePackageCommandHandler(ICoachPackageRepository packageRepository, CoachDbContext context)
        {
            _packageRepository = packageRepository;
            _context = context;
        }

        public async Task<UpdatePackageResult> Handle(UpdatePackageCommand command, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetCoachPackageByIdAsync(command.PackageId, cancellationToken);
            if (package == null)
                throw new NotFoundException($"Package with ID {command.PackageId} not found");

            if (package.CoachId != command.CoachId)
                throw new UnauthorizedAccessException("You are not authorized to update this package");

            package.Name = command.Name;
            package.Description = command.Description;
            package.Price = command.Price;
            package.SessionCount = command.SessionCount;
            package.Status = command.Status;
            package.UpdatedAt = DateTime.UtcNow;

            await _packageRepository.UpdateCoachPackageAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdatePackageResult(
                package.Id,
                package.Name,
                package.Description,
                package.Price,
                package.SessionCount,
                package.Status
            );
        }
    }
}
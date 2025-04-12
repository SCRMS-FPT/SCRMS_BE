using Coach.API.Data;
using Coach.API.Data.Repositories;
using BuildingBlocks.Exceptions;

namespace Coach.API.Features.Packages.DeletePackage
{
    public record DeletePackageResult(
        Guid Id,
        string Status,
        string Message
    );

    public record DeletePackageCommand(
        Guid PackageId,
        Guid CoachId
    ) : ICommand<DeletePackageResult>;

    public class DeletePackageCommandHandler : ICommandHandler<DeletePackageCommand, DeletePackageResult>
    {
        private readonly ICoachPackageRepository _packageRepository;
        private readonly CoachDbContext _context;

        public DeletePackageCommandHandler(ICoachPackageRepository packageRepository, CoachDbContext context)
        {
            _packageRepository = packageRepository;
            _context = context;
        }

        public async Task<DeletePackageResult> Handle(DeletePackageCommand command, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetCoachPackageByIdAsync(command.PackageId, cancellationToken);
            if (package == null)
                throw new NotFoundException($"Package with ID {command.PackageId} not found");

            if (package.CoachId != command.CoachId)
                throw new UnauthorizedAccessException("You are not authorized to delete this package");

            // Instead of actually deleting, we deactivate the package
            package.Status = "inactive";
            package.UpdatedAt = DateTime.UtcNow;

            await _packageRepository.UpdateCoachPackageAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new DeletePackageResult(
                package.Id,
                package.Status,
                "Package successfully deactivated"
            );
        }
    }
}
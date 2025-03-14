using BuildingBlocks.Exceptions;
using Coach.API.Data.Models;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Coach.API.Features.Packages.PurchasePackage
{
    public record PurchasePackageCommand(
        Guid UserId,
        Guid PackageId) : ICommand<PurchasePackageResult>;

    public record PurchasePackageResult(
        Guid Id,
        Guid CoachPackageId,
        DateTime PurchaseDate,
        DateTime ExpiryDate,
        int SessionCount,
        int SessionUsed);

    public class PurchasePackageCommandValidator : AbstractValidator<PurchasePackageCommand>
    {
        public PurchasePackageCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User id is required");
            RuleFor(x => x.PackageId).NotEmpty().WithMessage("Package id is required");
        }
    }

    public class PurchasePackageCommandHandler(CoachDbContext context)
        : ICommandHandler<PurchasePackageCommand, PurchasePackageResult>
    {
        public async Task<PurchasePackageResult> Handle(PurchasePackageCommand command, CancellationToken cancellationToken)
        {
            // save
            var package = await context.CoachPackages.FirstOrDefaultAsync(cp => cp.Id == command.PackageId);

            if (package == null)
            {
                throw new NotFoundException("CoachPackage", command.PackageId);
            }

            var purchase = new CoachPackagePurchase
            {
                UserId = command.UserId,
                CoachPackageId = command.PackageId,
                // TODO: Assumption must resolves
                ExpiryDate = DateTime.UtcNow.AddDays(package.SessionCount)
            };

            context.CoachPackagePurchases.Add(purchase);
            await context.SaveChangesAsync(cancellationToken);

            return new PurchasePackageResult(purchase.Id, purchase.CoachPackageId, purchase.PurchaseDate, purchase.ExpiryDate, package.SessionCount, purchase.SessionsUsed);
        }
    }
}
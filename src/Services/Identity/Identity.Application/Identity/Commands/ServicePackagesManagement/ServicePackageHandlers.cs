using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.ServicePackagesManagement
{
    public class ServicePackageHandlers :
        ICommandHandler<CreateServicePackageCommand, ServicePackageDto>,
        ICommandHandler<UpdateServicePackageCommand, ServicePackageDto>,
        ICommandHandler<DeleteServicePackageCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ServicePackageHandlers(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ServicePackageDto> Handle(
            CreateServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = ServicePackage.Create(
                request.Name,
                request.Description,
                request.Price,
                request.DurationDays);

            _context.ServicePackages.Add(package);
            await _context.SaveChangesAsync(cancellationToken);

            return package.Adapt<ServicePackageDto>();
        }

        public async Task<ServicePackageDto> Handle(
            UpdateServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = await _context.ServicePackages
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (package == null) throw new NotFoundException(nameof(ServicePackage), request.Id);

            package.UpdateDetails(
                request.Name,
                request.Description,
                request.Price,
                request.DurationDays);

            await _context.SaveChangesAsync(cancellationToken);

            return package.Adapt<ServicePackageDto>();
        }

        public async Task<Unit> Handle(
            DeleteServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = await _context.ServicePackages
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (package == null) throw new NotFoundException(nameof(ServicePackage), request.Id);

            // Check if any subscriptions exist
            if (await _context.Subscriptions.AnyAsync(x => x.ServicePackageId == request.Id))
            {
                throw new ConflictException("Cannot delete package with active subscriptions");
            }

            _context.ServicePackages.Remove(package);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

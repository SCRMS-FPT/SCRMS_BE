using Identity.Application.Data;
using Identity.Application.Data.Repositories;
using Identity.Domain.Exceptions;
using Mapster;

namespace Identity.Application.ServicePackages.Commands.ServicePackagesManagement
{
    public class ServicePackageHandlers :
        ICommandHandler<CreateServicePackageCommand, ServicePackageDto>,
        ICommandHandler<UpdateServicePackageCommand, ServicePackageDto>,
        ICommandHandler<DeleteServicePackageCommand, Unit>
    {
        private readonly IServicePackageRepository _packageRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public ServicePackageHandlers(
            IServicePackageRepository packageRepository,
            ISubscriptionRepository subscriptionRepository)
        {
            _packageRepository = packageRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<ServicePackageDto> Handle(
            CreateServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = ServicePackage.Create(
                request.Name,
                request.Description,
                request.Price,
                request.DurationDays,
                request.AssociatedRole,
                request.Status ?? "active");

            await _packageRepository.AddServicePackageAsync(package);

            return package.Adapt<ServicePackageDto>();
        }

        public async Task<ServicePackageDto> Handle(
            UpdateServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetServicePackageByIdAsync(request.Id);
            if (package == null)
                throw new NotFoundException(nameof(ServicePackage), request.Id);

            package.UpdateDetails(
                request.Name,
                request.Description,
                request.Price,
                request.DurationDays,
                request.AssociatedRole,
                request.Status);

            await _packageRepository.UpdateServicePackageAsync(package);

            return package.Adapt<ServicePackageDto>();
        }

        public async Task<Unit> Handle(
            DeleteServicePackageCommand request,
            CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetServicePackageByIdAsync(request.Id);
            if (package == null)
                throw new NotFoundException(nameof(ServicePackage), request.Id);

            if (await _subscriptionRepository.ExistsSubscriptionByPackageIdAsync(request.Id))
            {
                throw new ConflictException("Cannot delete package with active subscriptions");
            }

            await _packageRepository.DeleteServicePackageAsync(package);

            return Unit.Value;
        }
    }
}
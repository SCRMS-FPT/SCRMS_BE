using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.ServicePackagesManagement
{
    public record CreateServicePackageCommand(
        string Name,
        string Description,
        decimal Price,
        int DurationDays
    ) : ICommand<ServicePackageDto>;

    public class CreateServicePackageValidator : AbstractValidator<CreateServicePackageCommand>
    {
        public CreateServicePackageValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.DurationDays).GreaterThan(0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.ServicePackagesManagement
{
    public record DeleteServicePackageCommand(int Id) : ICommand<Unit>;
}

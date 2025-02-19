using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.Role
{
    public record AssignRolesToUserCommand(Guid UserId, List<string> Roles) : ICommand<Unit>;
}

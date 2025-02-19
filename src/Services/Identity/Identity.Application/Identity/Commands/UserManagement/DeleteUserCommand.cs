using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Commands.UserManagement
{
    public record DeleteUserCommand(Guid UserId) : ICommand<Unit>;

}

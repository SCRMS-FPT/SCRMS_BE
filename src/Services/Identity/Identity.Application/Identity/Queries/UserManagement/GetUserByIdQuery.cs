using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto?>;
}

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Queries.UserManagement
{
    public record GetUsersQuery : IQuery<IEnumerable<UserDto>>;

    
}

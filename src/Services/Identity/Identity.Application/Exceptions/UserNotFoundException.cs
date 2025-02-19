using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class UserNotFoundException(Guid userId)
        : Exception($"User with ID {userId} not found");
}

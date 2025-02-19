using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class NotFoundException(string entity, object key)
       : Exception($"{entity} with id {key} not found");
}

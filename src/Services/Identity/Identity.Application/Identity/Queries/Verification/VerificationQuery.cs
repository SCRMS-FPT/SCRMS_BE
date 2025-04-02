using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Identity.Queries.Verification
{
    public record VerificationQuery(string Token) : IQuery<Unit>;
}

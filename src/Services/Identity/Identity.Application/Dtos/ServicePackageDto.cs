using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Dtos
{
    public record ServicePackageDto(
        int Id,
        string Name,
        string Description,
        decimal Price,
        int DurationDays,
        DateTime CreatedAt,
        int TotalSubscriptions
    );
}

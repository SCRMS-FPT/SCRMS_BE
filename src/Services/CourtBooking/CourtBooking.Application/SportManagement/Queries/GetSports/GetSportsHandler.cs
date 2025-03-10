using MediatR;
using Microsoft.EntityFrameworkCore;
using CourtBooking.Application.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CourtBooking.Application.SportManagement.Queries.GetSports;

public class GetSportsHandler(IApplicationDbContext _context)
    : IRequestHandler<GetSportsQuery, GetSportsResult>
{

    public async Task<GetSportsResult> Handle(GetSportsQuery request, CancellationToken cancellationToken)
    {
        var sports = await _context.Sports
            .Select(s => new SportDTO(
                s.Id.Value,
                s.Name,
                s.Description,
                s.Icon
            ))
            .ToListAsync(cancellationToken);

        return new GetSportsResult(sports);
    }
}

using BuildingBlocks.Pagination;
using CourtBooking.Application.DTOs;
using CourtBooking.Application.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CourtBooking.Application.CourtManagement.Queries.GetCourts;

public class GetCourtsHandler(IApplicationDbContext _context) 
    : IQueryHandler<GetCourtsQuery, GetCourtsResult>
{
    public async Task<GetCourtsResult> Handle(GetCourtsQuery query, CancellationToken cancellationToken)
    {
        // Get all courts with pagination
        var pageIndex = query.PaginationRequest.PageIndex;
        var pageSize = query.PaginationRequest.PageSize;

        var totalCount = await _context.Courts.LongCountAsync(cancellationToken);

        var courts = await _context.Courts
                       .Include(o => o.OperatingHours)
                       .Include(o => o.Sport)
                       .OrderBy(o => o.CourtName.Value)
                       .Skip(pageSize * pageIndex)
                       .Take(pageSize)
                       .ToListAsync(cancellationToken);

        return new GetCourtsResult(
    new PaginatedResult<CourtDTO>(
        pageIndex,
        pageSize,
        totalCount,
        courts.ToCourtDtoList()));

    }
}
